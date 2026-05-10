FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskManager.sln .
COPY TaskManager.API/TaskManager.API.csproj TaskManager.API/
COPY TaskManager.Tests/TaskManager.Tests.csproj TaskManager.Tests/

RUN dotnet restore

COPY . .

RUN dotnet publish TaskManager.API/TaskManager.API.csproj -c Release -o /app/publish

# ── Runtime image ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install curl for HEALTHCHECK; create non-root user
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && addgroup --system --gid 1001 appgroup \
    && adduser  --system --uid 1001 --gid 1001 --no-create-home appuser

WORKDIR /app
COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=20s \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
