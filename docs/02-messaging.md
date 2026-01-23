# Messaging Architecture

The system uses asynchronous messaging through RabbitMQ for communication between microservices.

## Message Abstractions

### Message Types

**[IMessage](../ManufacturingOptimization.Common.Messaging/Abstractions/IMessage.cs)** — base interface for all messages. Contains a creation timestamp.

**[ICommand](../ManufacturingOptimization.Common.Messaging/Abstractions/ICommand.cs)** — a command, an instruction to perform an action. Inherits `IMessage`, adds a unique `CommandId` for tracking.

**[IEvent](../ManufacturingOptimization.Common.Messaging/Abstractions/IEvent.cs)** — an event, a notification that something has occurred in the system. Reaction to a Command. Inherits `IMessage`, contains an `EventId` and optional `CorrelationId` to link to the command that triggered the event.

**[IRequestReplyCommand](../ManufacturingOptimization.Common.Messaging/Abstractions/IRequestReplyCommand.cs)** — a command with response expectation (request-reply pattern). Inherits `ICommand`, adds a `ReplyTo` field with the reply queue name.

### Usage

Concrete commands and events inherit these interfaces.

For convenience, there are base classes [BaseEvent](../ManufacturingOptimization.Common.Messaging/Abstractions/BaseEvent.cs), [BaseCommand](../ManufacturingOptimization.Common.Messaging/Abstractions/BaseCommand.cs), [BaseRequestReplyCommand](../ManufacturingOptimization.Common.Messaging/Abstractions/BaseRequestReplyCommand.cs) that automatically generate IDs and set timestamps.

## Infrastructure Interfaces

**[IMessagePublisher](../ManufacturingOptimization.Common.Messaging/Abstractions/IMessagePublisher.cs)** — message publishing:
- `Publish()` — send a message to an exchange with a routing key
- `RequestReplyAsync()` — send a request with response expectation (RPC)
- `PublishReply()` — send a reply to an `IRequestReplyCommand` with automatic `CorrelationId` setting

**[IMessageSubscriber](../ManufacturingOptimization.Common.Messaging/Abstractions/IMessageSubscriber.cs)** — message subscription:
- `Subscribe()` — register a handler for a queue

**[IMessagingInfrastructure](../ManufacturingOptimization.Common.Messaging/Abstractions/IMessagingInfrastructure.cs)** — RabbitMQ topology management:
- `DeclareExchange()` — create an exchange
- `DeclareQueue()` — create a queue
- `BindQueue()` — bind a queue to an exchange with a routing key
- `PurgeQueue()` — clear a queue

## Implementation — RabbitMqService

[RabbitMqService](../ManufacturingOptimization.Common.Messaging/RabbitMqService.cs) implements all three interfaces (`IMessagePublisher`, `IMessageSubscriber`, `IMessagingInfrastructure`).

This is the single point of interaction with RabbitMQ:
- Establishes connection to RabbitMQ through [RabbitMqSettings](../ManufacturingOptimization.Common.Messaging/RabbitMqSettings.cs)
- Serializes/deserializes messages to/from JSON
- Manages subscriber lifecycle
- Implements RPC pattern through temporary reply queues

## Routing Constants

[Exchanges.cs](../ManufacturingOptimization.Common.Messaging/Messages/Exchanges.cs) contains exchange names:

Routing keys are organized by domains:
- [OptimizationRoutingKeys.cs](../ManufacturingOptimization.Common.Messaging/Messages/OptimizationRoutingKeys.cs) — keys for optimization (`optimization.plan-requested`, `optimization.plan-ready`)
- [ProcessRoutingKeys.cs](../ManufacturingOptimization.Common.Messaging/Messages/ProcessRoutingKeys.cs) — keys for processes
- [ProviderRoutingKeys.cs](../ManufacturingOptimization.Common.Messaging/Messages/ProviderRoutingKeys.cs) — keys for providers
- [SystemRoutingKeys.cs](../ManufacturingOptimization.Common.Messaging/Messages/SystemRoutingKeys.cs) — system keys

## Usage in Components

Typical messaging workflow:

1. **DI Registration:** `RabbitMqService` is registered as a singleton and injected through interfaces
2. **Topology Creation:** on startup, a component declares exchanges, queues, and bindings through `IMessagingInfrastructure`
3. **Subscription:** component registers handlers through `IMessageSubscriber.Subscribe()`
4. **Publishing:** component sends messages through `IMessagePublisher.Publish()`

Example:
```csharp
// Queue creation and binding
_infrastructure.DeclareQueue("engine.optimization");
_infrastructure.BindQueue("engine.optimization", Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested);

// Message subscription
_subscriber.Subscribe<PlanRequestedEvent>("engine.optimization", HandlePlanRequest);

// Event publishing
_publisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanReady, new PlanReadyEvent { ... });
```
