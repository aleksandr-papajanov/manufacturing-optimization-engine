using System;
using System.Collections.Generic;

namespace ManufacturingOptimization.Engine
{
    // The Internal Provider Model
    public class Provider
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Capabilities Capabilities { get; set; } = new();
    }

    // The Capabilities we are validating against
    public class Capabilities
    {
        public double MaxPowerKW { get; set; }
        public List<string> SupportedTypes { get; set; } = new();
    }
}