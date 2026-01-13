using System;
using Common.Models;

namespace Common.Messaging
{
    public class CustomerRequestSubmittedEvent : IEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // The Data payload
        public MotorRequest Request { get; set; }

        // Constructor for convenience
        public CustomerRequestSubmittedEvent(MotorRequest request)
        {
            Request = request;
        }
    }
}