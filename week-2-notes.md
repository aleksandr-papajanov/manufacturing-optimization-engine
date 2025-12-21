# Week 2 - Work Report

## Docker Orchestration: Two Modes

### Problem

The assignment requires dynamic provider management: add, remove, pause, start. This can be implemented via Docker API (Docker.DotNet), but there's a challenge.

Docker API works with images. Any changes to provider simulator code require rebuilding the Docker image before creating containers. This is fine for production but inconvenient for development.

When developing provider simulators, we want:
- Hot reload
- Debugger

### Solution

Two different implementations of the provider orchestrator:

1. **DockerProviderOrchestrator** - production mode
   - Uses Docker API to dynamically create/delete containers
   - Works with pre-built `provider-simulator:latest` image
   - Full lifecycle management

2. **ComposeManagedOrchestrator** - development mode
   - Containers managed by docker-compose, not programmatically
   - Start/Stop methods just log messages
   - Enables hot reload and VS debugger

Orchestrator selection via `Orchestration__Mode` environment variable in `ProviderRegistry`:
- `Production` → `DockerProviderOrchestrator`
- `Development` → `ComposeManagedOrchestrator`

### Running Production Mode

**Important:** Build provider image before first run!

1. **Build provider image** (once or after ProviderSimulator changes):
   - Run PowerShell script `build-provider-image.ps1` in solution root
   - Or execute: `docker build -t provider-simulator:latest -f ManufacturingOptimization.ProviderSimulator/Dockerfile .`

2. Set **docker-compose** as startup project in Visual Studio

3. Run (F5)

4. What happens:
   - Main stack from `docker-compose.yml` starts: RabbitMQ, Gateway, Engine, ProviderRegistry (Production mode)
   - ProviderRegistry reads `providers.json` and creates provider containers via Docker API
   - Providers start, publish `ProviderRegisteredEvent`, system ready

Without pre-built image, production mode fails (DockerProviderOrchestrator won't find `provider-simulator:latest`).

### Running Development Mode

1. Rename `docker-compose.dev.yml` to `docker-compose.override.yml` (auto-applied on top of main compose file)
2. Run docker-compose in Visual Studio
3. What happens:
   - Main `docker-compose.yml` applies
   - `docker-compose.override.yml` overlays:
     - Switches `Orchestration__Mode=Development`
     - Adds three pre-configured provider containers
   - ProviderRegistry uses `ComposeManagedOrchestrator` (logs only, no actions)
   - Providers run as normal compose services with hot reload and debugger access

### Running Console Application

Console app doesn't run in container:

1. Set **docker-compose** as startup project
2. Run (F5)
3. When containers start, right-click **ManufacturingOptimization.Console**
4. Select **Debug → Start New Instance**

---

## Solution Structure

### ManufacturingOptimization.ProviderSimulator

Simulates technology providers (production firms).

**Key points:**
- Single interface `IProviderSimulator` with three implementations: `MainRemanufacturingCenter`, `EngineeringDesignFirm`, `PrecisionMachineShop`
- Current logic: random accept/decline responses (placeholder)
- Publishes `ProviderRegisteredEvent` on startup

**Configuration via environment variables:**
- All config from Docker environment variables
- `Program.cs` maps vars to Settings classes via IOptions pattern
- Each provider has own Settings class (e.g., `MainRemanufacturingCenterSettings`)
- `RabbitMqSettings` also from environment variables
- Only way to configure (each project runs in isolated container)

### ManufacturingOptimization.ProviderRegistry

Provider registry and orchestrator. Manages container lifecycle.

**Responsibilities:**
- Store provider information
- Start/stop provider containers (via Docker API in production)
- Handle provider management commands

**Data source:**
- Currently: `providers.json` (simple JSON file)
- Future: database

**Event handling:**
- Subscribes to `ProviderRegisteredEvent`
- Saves registered providers to in-memory repository

### ManufacturingOptimization.Engine

Optimization engine. Main business logic here.

**Current implementation (simplified):**
- Subscribes to `RequestOptimizationPlanCommand`
- Selects random registered provider
- Sends `ProposeProcessCommand` to provider
- Receives response (`ProcessAcceptedEvent` or `ProcessDeclinedEvent`)
- Publishes `OptimizationPlanCreatedEvent`

Future: real optimization logic, criteria-based provider selection, rejection handling, etc. Currently just a test stub.

### ManufacturingOptimization.Gateway

API Gateway - main client entry point.

**Functions:**
- Accepts HTTP requests (REST API)
- Publishes commands to RabbitMQ
- Subscribes to result events
- Stores results in in-memory `IRequestResponseRepository`
- Returns HTTP responses

Gateway isolates clients from internal event-driven architecture. Clients use simple REST API, internally everything works via RabbitMQ.

### ManufacturingOptimization.Console

Console app for testing. Can be deleted in production.

### Common.Messaging

Shared RabbitMQ infrastructure library.

**Contents:**

1. **Message contracts:**
   - `IMessage` - base marker for all messages
   - `ICommand` - commands (action requests), contain `CommandId`
   - `IEvent` - events (command reactions), contain `CommandId` for correlation

2. **RabbitMQ constants:** routing keys, exchange names, queue names

3. **Settings:** `RabbitMqSettings` (host, port, username, password from environment)

4. **Service:** `RabbitMqService` - implements `IMessagePublisher`, `IMessageSubscriber`, `IMessagingInfrastructure`
   - Allows DI injection of only needed interface (publisher or subscriber)

### Common.Models

Shared data models. Minimal set currently.

---

## Current Demo Flow

**Note:** Current implementation is demo/proof-of-concept to verify component communication. Everything can be refactored.

### 1. System Startup

**Engine** starts as background service (`EngineWorker`).

**ProviderRegistry** starts:
- Reads `Orchestration__Mode` environment variable
- Creates appropriate orchestrator (`DockerProviderOrchestrator` or `ComposeManagedOrchestrator`)
- Calls `StartAllAsync()`

**Production mode:**
- Reads `providers.json`
- Creates containers via Docker API
- Passes environment variables (PROVIDER_TYPE, RabbitMQ settings, provider Settings)

**Development mode:**
- Containers already running via `docker-compose.override.yml`
- Orchestrator just logs

### 2. Provider Registration

Each provider on startup:
- Publishes `ProviderRegisteredEvent` to RabbitMQ with `ProviderId`, `ProviderName`, `ProviderType`

Components save to in-memory repositories:
- **Gateway** → `IProviderRepository`
- **Engine** → `IProviderRepository`

All components now know available providers.

### 3. Request Provider List (via Console)

**Console** → HTTP GET `/api/providers` to Gateway

**Gateway** → returns data from in-memory `IProviderRepository`

### 4. Request Optimization (via Console)

**Console** → HTTP POST `/api/optimization` to Gateway

**Gateway** → publishes `RequestOptimizationPlanCommand` to RabbitMQ

**Engine** → receives command:
1. Gets registered providers from `IProviderRepository`
2. Selects **random** provider (stub logic)
3. Publishes `ProposeProcessCommand` to selected provider

**Provider** → receives `ProposeProcessCommand`:
1. Randomly decides accept/decline
2. Publishes `ProcessAcceptedEvent` or `ProcessDeclinedEvent`

**Engine** → receives provider response:
1. Publishes `OptimizationPlanCreatedEvent` with:
   - `CommandId` (correlation with original request)
   - `ProviderId` (who responded)
   - `Response` ("accepted" or "declined")

**Gateway** → `GatewayWorker` (hosted service) listens to `OptimizationPlanCreatedEvent`:
1. Receives event
2. Saves result to in-memory `IRequestResponseRepository` (key: `CommandId`)

### 5. Get Result (via Console)

**Console** → waits a few seconds, then HTTP GET `/api/optimization/{commandId}`

**Gateway** → searches `IRequestResponseRepository` by `CommandId`:
- Found → returns provider info and response
- Not found → returns 404

**Console** → displays which provider accepted/declined

---

## Current Limitations & Plans

**In-memory storage:**
- Current: `ConcurrentDictionary` (lost on restart)
- Plan: real database (SQL Server/PostgreSQL)

**Engine logic:**
- Current: random provider selection, send to one
- Plan: smart criteria-based selection, send to multiple, choose best offer

**Providers:**
- Current: random accept/decline
- Plan: real production process simulation logic

**Provider configuration:**
- Current: `providers.json` file
- Plan: UI for add/remove providers, database storage

Everything is proof-of-concept for event-driven architecture and RabbitMQ communication between containers.

---