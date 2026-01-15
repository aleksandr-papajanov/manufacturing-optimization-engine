using System;
using System.Collections.Generic;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Engine
{
    // The Event from RabbitMQ
    public class ProviderRegisteredEvent : IMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ProviderId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty; 
    }

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