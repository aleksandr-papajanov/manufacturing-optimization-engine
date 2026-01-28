namespace ManufacturingOptimization.Gateway.Exceptions
{
    public class ServiceNotReadyException : GatewayException
    {
        public ServiceNotReadyException(string message = "System is still initializing. Please try again in a few moments.") 
            : base(message, 503)
        {
        }
    }
}
