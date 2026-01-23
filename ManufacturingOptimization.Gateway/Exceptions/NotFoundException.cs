namespace ManufacturingOptimization.Gateway.Exceptions
{
    public class NotFoundException : GatewayException
    {
        public NotFoundException(string message) 
            : base(message, 404)
        {
        }
    }
}
