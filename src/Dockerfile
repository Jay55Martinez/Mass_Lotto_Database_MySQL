# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet build src.csproj -c Release -o /app/build

# Publish stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS publish
WORKDIR /src
COPY --from=build /src .
RUN dotnet publish src.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Copy configuration files
COPY appsettings.json .

# Set environment variable for connection string (will be overridden by docker-compose)
ENV DB_CONNECTION_STRING=""

ENTRYPOINT ["dotnet", "src.dll"]