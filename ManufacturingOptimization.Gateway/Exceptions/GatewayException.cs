namespace ManufacturingOptimization.Gateway.Exceptions
{
    public class GatewayException : Exception
    {
        public int StatusCode { get; }

        public GatewayException(string message, int statusCode = 500) 
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
