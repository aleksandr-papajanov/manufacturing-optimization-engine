# Week 2 - Work Report

**Note:** "Provider" refers to Technology Provider - a manufacturing firm/facility (e.g., remanufacturing center, engineering firm, machine shop).

## Docker Orchestration: Two Modes

### Problem

The system needs to dynamically manage technology providers: add, remove, pause, restart based on business requirements. However, during development of provider simulators themselves, we need debugger access and hot reload.

These requirements conflict:
- **Dynamic management** requires providers running in Docker containers managed via Docker API
- **Provider development** requires debugger access and hot reload, which needs running code directly from IDE

Using Docker API means rebuilding images on every code change. Using docker-compose with static containers means no dynamic management.

### Solution

Two orchestration modes with different trade-offs:

**Development Mode** - for developing provider simulators:
- Providers defined in `docker-compose.dev.yml` as static services
- All containers managed by docker-compose
- Full debugger and hot reload support
- **Cannot** dynamically add/remove/pause providers
- Orchestrator: `ComposeManagedOrchestrator` (logs actions, does nothing)

**Production Mode** - for everything except provider development:
- Providers created dynamically via Docker API
- Provider list from `providers.json` (future: database)
- Can add/remove/pause providers programmatically
- **Cannot** debug provider code (containers run pre-built images)
- Orchestrator: `DockerProviderOrchestrator` (full lifecycle management)

Mode selection via `Orchestration__Mode` environment variable in ProviderRegistry.

### Running Production Mode

**Important:** Build provider image before first run!

1. **Build provider image** (once or after ProviderSimulator changes):
   - Run PowerShell script `build-provider-image.ps1` in solution root
   - Or execute: `docker build -t provider-simulator:latest -f ManufacturingOptimization.ProviderSimulator/Dockerfile .`

2. Set **docker-compose** as startup project in Visual Studio

3. Run (F5)

4. What happens:
   - Main stack from `docker-compose.yml` starts: RabbitMQ, Gateway, Engine, ProviderRegistry (Production mode)
   - ProviderRegistry reads `providers.json` and creates technology provider containers via Docker API
   - Technology providers start, publish `ProviderRegisteredEvent`, system ready

Without pre-built image, production mode fails (DockerProviderOrchestrator won't find `provider-simulator:latest`).

### Running Development Mode

1. Rename `docker-compose.dev.yml` to `docker-compose.override.yml` (it will be auto-applied on top of main compose file)
2. Run docker-compose in Visual Studio
3. What happens:
   - Main `docker-compose.yml` applies
   - `docker-compose.override.yml` overlays:
     - Switches `Orchestration__Mode=Development`
     - Adds three pre-configured technology provider containers
   - ProviderRegistry uses `ComposeManagedOrchestrator` (logs only, no actions)
   - Technology providers run as normal compose services with hot reload and debugger access

### Running Console Application

Console app doesn't run in container:

1. Set **docker-compose** as startup project
2. Run (F5)
3. When containers start, right-click **ManufacturingOptimization.Console**
4. Select **Debug → Start New Instance**

---

## Solution Structure

**ManufacturingOptimization.ProviderSimulator** - Simulates technology providers (manufacturing firms). Three implementations (`MainRemanufacturingCenter`, `EngineeringDesignFirm`, `PrecisionMachineShop`) with random accept/decline logic. Publishes `ProviderRegisteredEvent` on startup. Configured via IOptions pattern from environment variables.

**ManufacturingOptimization.ProviderRegistry** - Manages technology provider lifecycle. Reads `providers.json`, creates/stops provider containers via Docker API (production) or uses docker-compose (development). Selects orchestrator based on `Orchestration__Mode`.

**ManufacturingOptimization.Engine** - Optimization logic. Currently: selects random technology provider, sends proposal, publishes result. Future: criteria-based selection, multiple providers.

**ManufacturingOptimization.Gateway** - REST API entry point. Converts HTTP requests to RabbitMQ commands, stores results, returns responses.

**ManufacturingOptimization.Console** - Testing tool. Runs outside Docker, uses Spectre.Console for UI.

**Common.Messaging** - RabbitMQ infrastructure. Message contracts (`ICommand`, `IEvent` with `CommandId` for correlation), `RabbitMqService` implementing publisher/subscriber interfaces.

**Common.Models** - Shared data models.

---

## Current Demo Flow

**Note:** Proof-of-concept implementation to verify component communication.

1. **Startup:** ProviderRegistry starts technology provider containers (Docker API in prod, compose in dev). Providers publish `ProviderRegisteredEvent`.

2. **Registration:** Gateway and Engine save registered technology providers to in-memory repositories.

3. **Request:** Console → Gateway `/api/optimization` → `RequestOptimizationPlanCommand` to RabbitMQ

4. **Processing:** Engine selects random technology provider → sends `ProposeProcessCommand` → Provider responds with `ProcessAcceptedEvent`/`ProcessDeclinedEvent`

5. **Response:** Engine publishes `OptimizationPlanCreatedEvent` → Gateway saves → Console retrieves result

---