FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TaskManager.sln .
COPY TaskManager.API/TaskManager.API.csproj TaskManager.API/
COPY TaskManager.Tests/TaskManager.Tests.csproj TaskManager.Tests/

RUN dotnet restore

COPY . .

RUN dotnet publish TaskManager.API/TaskManager.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "TaskManager.API.dll"]