namespace ManufacturingOptimization.Gateway.DTOs
{
    public class ProvidersResponse
    {
        public int TotalProviders { get; set; }
        public List<ProviderDto> Providers { get; set; } = new();
    }
}
