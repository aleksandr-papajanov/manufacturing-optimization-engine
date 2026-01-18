namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class ProviderRoutingKeys
{
    public const string RequestRegistrationAll = "provider.start-all";
    public const string Registered = "provider.registered";
    public const string AllRegistered = "provider.all-ready";
    
    // Provider validation flow (US-11)
    public const string ValidationRequested = "provider.validation.requested";
    public const string ValidationApproved = "provider.validation.approved";
    public const string ValidationDeclined = "provider.validation.declined";
}
