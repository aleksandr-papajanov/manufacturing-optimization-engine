using System;

namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProcessCapabilityDto
{
    public Guid Id { get; set; }
    public string Process { get; set; } = string.Empty;
    public decimal CostPerHour { get; set; }
    public double SpeedMultiplier { get; set; } = 1.0;
    public double QualityScore { get; set; } = 0.8;
    public double EnergyConsumptionKwhPerHour { get; set; }
    public double CarbonIntensityKgCO2PerKwh { get; set; } = 0.5;
    public bool UsesRenewableEnergy { get; set; }
}
