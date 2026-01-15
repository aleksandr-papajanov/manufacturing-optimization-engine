# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy Solution and Project files
COPY ["ManufacturingOptimization.sln", "./"]
COPY ["ManufacturingOptimization.Analytics/ManufacturingOptimization.Analytics.csproj", "ManufacturingOptimization.Analytics/"]
COPY ["ManufacturingOptimization.Common.Models/ManufacturingOptimization.Common.Models.csproj", "ManufacturingOptimization.Common.Models/"]
COPY ["ManufacturingOptimization.Common.Messaging/ManufacturingOptimization.Common.Messaging.csproj", "ManufacturingOptimization.Common.Messaging/"]
COPY ["ManufacturingOptimization.Engine/ManufacturingOptimization.Engine.csproj", "ManufacturingOptimization.Engine/"]
COPY ["ManufacturingOptimization.ProviderRegistry/ManufacturingOptimization.ProviderRegistry.csproj", "ManufacturingOptimization.ProviderRegistry/"]
COPY ["ManufacturingOptimization.Gateway/ManufacturingOptimization.Gateway.csproj", "ManufacturingOptimization.Gateway/"]

# Restore dependencies
RUN dotnet restore "ManufacturingOptimization.Analytics/ManufacturingOptimization.Analytics.csproj"

# Copy the rest of the source code
COPY . .

# Build the project
WORKDIR "/src/ManufacturingOptimization.Analytics"
RUN dotnet build "ManufacturingOptimization.Analytics.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManufacturingOptimization.Analytics.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManufacturingOptimization.Analytics.dll"]