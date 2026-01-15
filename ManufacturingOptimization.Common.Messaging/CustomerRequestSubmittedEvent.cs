using System;
using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging
{
    public class CustomerRequestSubmittedEvent : IMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string RequestId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;

        // FIX: Use simple 'double' instead of 'MotorRequest' object to fix build errors
        public double RequiredPowerKW { get; set; }
    }
}