# System Startup Synchronization (System Readiness)

_(User Story: [US-20](project-overview.md#epic-5-platform-features--extensibility) — Platform extensibility, [US-26](project-overview.md#epic-7-infrastructure--deployment) — Containerization)_

When the system starts up, multiple microservices must coordinate their startup to ensure correct initialization of all components before request processing begins. The **System Readiness** mechanism ensures startup synchronization through event exchange and execution blocking.

## Core Components

### SystemReadinessService

[SystemReadinessService](../ManufacturingOptimization.Common.Messaging/SystemReadinessService.cs) — base service in `Common.Messaging`, implementing the [ISystemReadinessService](../ManufacturingOptimization.Common.Messaging/Abstractions/ISystemReadinessService.cs) interface.

**Key capabilities:**
- **`WaitForSystemReadyAsync()`** — blocks execution via `TaskCompletionSource` until receiving `SystemReadyEvent`
- **`MarkSystemReady()`** — sets `TaskCompletionSource`, unblocking all waiting threads
- **Subscription to `SystemReadyEvent`** — automatically calls `MarkSystemReady()` when event is received

### StartupCoordinator

[StartupCoordinator](../ManufacturingOptimization.Engine/Services/StartupCoordinator.cs) — extension of `SystemReadinessService` in Engine, acts as the coordinator.

**Functions:**
- Stores list of required services: `Gateway`, `ProviderRegistry`, `Engine`
- Subscribes to [ServiceReadyEvent](../ManufacturingOptimization.Common.Messaging/Messages/SystemManagement/ServiceReadyEvent.cs) events
- Tracks which services have reported readiness
- Publishes [SystemReadyEvent](../ManufacturingOptimization.Common.Messaging/Messages/SystemManagement/SystemReadyEvent.cs) when all required services have started

## Workflow Sequence

### Startup Synchronization Diagram

![Service Startup Coordination](assets/system-readiness/service-startup-coordination.png)

_[Source PlantUML](assets/system-readiness/service-startup-coordination.puml)_

### 1. Microservice Startup (e.g., ProviderRegistry)

When [ProviderRegistryWorker](../ManufacturingOptimization.ProviderRegistry/ProviderRegistryWorker.cs) starts:

```csharp
protected override async Task ExecuteAsync(CancellationToken cancellationToken)
{
    SetupRabbitMq();  // Setup queues and subscriptions
    
    await Task.Delay(1000, cancellationToken);  // Give time for subscription registration
    
    // Publish readiness event
    var readyEvent = new ServiceReadyEvent { ServiceName = "ProviderRegistry" };
    _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, readyEvent);
    
    // Block until entire system is ready
    await _readinessService.WaitForSystemReadyAsync(cancellationToken);
    
    // Continue execution only after unblocking
    await _orchestrator.StartAllAsync(cancellationToken);
}
```

**Similarly work:**
- [EngineWorker](../ManufacturingOptimization.Engine/EngineWorker.cs) — publishes `ServiceReadyEvent` for `Engine`
- Gateway — publishes `ServiceReadyEvent` for `Gateway`

### 2. Coordination in StartupCoordinator

`StartupCoordinator` in Engine receives `ServiceReadyEvent` events:

```csharp
private void HandleServiceReady(ServiceReadyEvent evt)
{
    lock (_readyServices)
    {
        _readyServices.Add(evt.ServiceName);  // Add to ready list
        CheckAndPublishSystemReady();         // Check and publish
    }
}

private void CheckAndPublishSystemReady()
{
    var allReady = REQUIRED_SERVICES.All(s => _readyServices.Contains(s));
    
    if (allReady)
    {
        // All required services are ready
        var evt = new SystemReadyEvent { ReadyServices = _readyServices.ToList() };
        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.SystemReady, evt);
    }
}
```

### 3. Service Unblocking

After `SystemReadyEvent` is published:

1. **SystemReadinessService** in each service receives the event
2. `HandleSystemReady()` → `MarkSystemReady()` is called
3. `TaskCompletionSource` is set to `true`
4. `WaitForSystemReadyAsync()` method completes
5. Code execution continues

## HTTP Endpoint Protection — SystemReadinessMiddleware

[SystemReadinessMiddleware](../ManufacturingOptimization.Gateway/Middleware/SystemReadinessMiddleware.cs) is registered in Gateway, which checks system readiness before processing HTTP requests.

**Operating principle:**
- Checks `ISystemReadinessService.IsSystemReady`
- If system is not ready, returns **HTTP 503 Service Unavailable** with JSON message
- If system is ready, passes request further down the pipeline

**Registration in Program.cs:**
```csharp
app.UseSystemReadiness(); // Added to middleware pipeline
```

This ensures that all incoming requests are correctly rejected until full system initialization is complete.

## Provider Startup and Registration Process

### Provider Registration Diagram

![Provider Registration Flow](assets/system-readiness/provider-registration-flow.png)

_[Source PlantUML](assets/system-readiness/provider-registration-flow.puml)_

After system unblocking (receiving `SystemReadyEvent`), provider initialization process starts in ProviderRegistry.

### Provider Startup — StartAllAsync

[ProviderRegistryWorker](../ManufacturingOptimization.ProviderRegistry/ProviderRegistryWorker.cs) calls `_orchestrator.StartAllAsync()` after `WaitForSystemReadyAsync()`.

Behavior depends on orchestration mode:

#### Development Mode

[ComposeManagedOrchestrator](../ManufacturingOptimization.ProviderRegistry/Services/ComposeManagedOrchestrator.cs) — providers are already started via docker-compose, so `RequestRegistrationAll` event is published immediately.

#### Production Mode

[DockerProviderOrchestrator](../ManufacturingOptimization.ProviderRegistry/Services/DockerProviderOrchestrator.cs) performs full validation and startup cycle:

1. **Get provider list** from repository
2. **Validate each provider** via [ProviderValidationService](../ManufacturingOptimization.ProviderRegistry/Services/ProviderValidationService.cs):
   - `ValidateProviderCapabilityCommand` command is published (routing key: `provider.validation-requested`)
   - Engine ([ProviderCapabilityValidationService](../ManufacturingOptimization.Engine/Services/ProviderCapabilityValidationService.cs)) processes the request
   - Provider capabilities and technical specifications are checked
   - `ProviderCapabilityValidatedEvent` event is published in response
3. **Start container** — if validation successful, `StartAsync()` is called, Docker container is created
4. **Publish `RequestRegistrationAll`** — after all providers are started

### Provider Registration

After `RequestRegistrationAll` event is published _(User Story: [US-01](project-overview.md#epic-1-technology-provider-management) — Provider registration)_:

1. **Providers receive event** — all [ProviderSimulator](../ManufacturingOptimization.ProviderSimulator/ProviderSimulatorWorker.cs) instances are subscribed to `RequestRegistrationAll`
2. **Publish `ProviderRegisteredEvent`** — each provider sends:
   - Its metadata (ID, name, type)
   - Technical capabilities (`TechnicalCapabilities`)
   - Process capabilities (`ProcessCapabilities`)
3. **Data storage** by system components:
   - **Gateway** — stores for serving via HTTP requests
   - **Engine** — stores for use in optimization and planning
   - **ProviderRegistry** — tracks for monitoring registration completion

4. **Publish `AllProvidersRegisteredEvent`** — when all started providers have registered, orchestrator publishes final event