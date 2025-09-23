# Multi-stage Dockerfile for .NET 9.0 Backend

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY ManagementSimulator/*.sln ./
COPY ManagementSimulator/ManagementSimulator.API/*.csproj ./ManagementSimulator.API/
COPY ManagementSimulator/ManagementSimulator.Core/*.csproj ./ManagementSimulator.Core/
COPY ManagementSimulator/ManagementSimulator.Database/*.csproj ./ManagementSimulator.Database/
COPY ManagementSimulator/ManagementSimulator.Infrastructure/*.csproj ./ManagementSimulator.Infrastructure/
COPY ManagementSimulator/ManagementSimulator.Tests/*.csproj ./ManagementSimulator.Tests/

# Restore NuGet packages
RUN dotnet restore

# Copy source code
COPY ManagementSimulator/ ./

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish ManagementSimulator.API/ManagementSimulator.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install SQLite (required for runtime)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/MGMTSimulator.db"

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "ManagementSimulator.API.dll"]