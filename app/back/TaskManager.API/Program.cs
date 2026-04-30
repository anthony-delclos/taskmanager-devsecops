using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Entities;
using TaskManager.API.Middleware;
using TaskManager.API.Repositories;
using TaskManager.API.Repositories.Interfaces;
using TaskManager.API.Services;
using TaskManager.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(",", StringSplitOptions.RemoveEmptyEntries)
            ?? new[] { "http://localhost:4200", "http://localhost:8080" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// A07 – brute-force / enumeration protection: 60 req/min per IP
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.RejectionStatusCode = 429;
});

builder.Services.AddEndpointsApiExplorer();
if (builder.Environment.IsDevelopment())
    builder.Services.AddSwaggerGen();

var app = builder.Build();

// A05 – no stack traces leaked to client; still include message in dev
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var message = app.Environment.IsDevelopment()
            ? feature?.Error.Message
            : "An internal error occurred.";
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// A05 – security headers on all normal responses
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();
app.UseCors("AllowAngular");

app.MapHealthChecks("/health").DisableRateLimiting();
app.MapControllers();
app.Run();

// Expose Program class for integration tests
public partial class Program { }
