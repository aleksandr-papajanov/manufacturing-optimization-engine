# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ManufacturingOptimization.sln", "./"]
COPY ["ManufacturingOptimization.ProviderRegistry/ManufacturingOptimization.ProviderRegistry.csproj", "ManufacturingOptimization.ProviderRegistry/"]
COPY ["ManufacturingOptimization.Common.Models/ManufacturingOptimization.Common.Models.csproj", "ManufacturingOptimization.Common.Models/"]
COPY ["ManufacturingOptimization.Common.Messaging/ManufacturingOptimization.Common.Messaging.csproj", "ManufacturingOptimization.Common.Messaging/"]

# Restore dependencies
RUN dotnet restore "ManufacturingOptimization.ProviderRegistry/ManufacturingOptimization.ProviderRegistry.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/ManufacturingOptimization.ProviderRegistry"
RUN dotnet build "ManufacturingOptimization.ProviderRegistry.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManufacturingOptimization.ProviderRegistry.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ManufacturingOptimization.ProviderRegistry.dll"]
