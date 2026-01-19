namespace ManufacturingOptimization.Gateway.DTOs
{
    public class ProviderDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public List<ProviderProcessCapabilityDto> ProcessCapabilities { get; set; } = new();
        public ProviderTechnicalCapabilitiesDto TechnicalCapabilities { get; set; } = new();
    }

    public class ProviderProcessCapabilityDto
    {
        public string ProcessName { get; set; } = string.Empty;
        public decimal CostPerHour { get; set; }
        public double SpeedMultiplier { get; set; }
        public double QualityScore { get; set; }
        public double EnergyConsumptionKwhPerHour { get; set; }
        public double CarbonIntensityKgCO2PerKwh { get; set; }
        public bool UsesRenewableEnergy { get; set; }
    }

    public class ProviderTechnicalCapabilitiesDto
    {
        public double AxisHeight { get; set; }
        public double Power { get; set; }
        public double Tolerance { get; set; }
    }
}
