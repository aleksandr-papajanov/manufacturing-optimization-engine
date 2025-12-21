# Script to build the provider simulator image
# Use WHEN:
# - Before the first run: .\build-provider-image.ps1
# - After changes in Dockerfile or dependencies
# - Before deploying to production
#
# FOR DEVELOPMENT: run providers locally from IDE (no need to rebuild)

Write-Host ' Building provider simulator image...' -ForegroundColor Green

docker build -t provider-simulator:latest `
    -f ManufacturingOptimization.ProviderSimulator/Dockerfile .

if ($LASTEXITCODE -eq 0) {
    Write-Host ' Image built successfully!' -ForegroundColor Green
    Write-Host ''
    Write-Host ' Image name: provider-simulator:latest' -ForegroundColor Cyan
    Write-Host ''
    docker images provider-simulator:latest
} else {
    Write-Host ' Build failed!' -ForegroundColor Red
    exit 1
}
