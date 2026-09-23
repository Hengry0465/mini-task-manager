# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaskManager.Api/*.csproj ./TaskManager.Api/
RUN dotnet restore ./TaskManager.Api/TaskManager.Api.csproj

COPY TaskManager.Api/. ./TaskManager.Api/
WORKDIR /src/TaskManager.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]