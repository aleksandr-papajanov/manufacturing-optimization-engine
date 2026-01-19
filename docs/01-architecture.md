# System Architecture

## Component Diagram Overview

## Solution Project Structure

The solution consists of the following projects:

### Microservices

- **ManufacturingOptimization.Gateway** — API Gateway, the entry point for client requests. Accepts HTTP requests, publishes events to RabbitMQ.
- **ManufacturingOptimization.Engine** — Engine, the main optimization module. Processes client requests, selects strategy (Upgrade/Refurbish), generates production plan.
- **ManufacturingOptimization.ProviderRegistry** — provider registry. Manages provider lifecycle, registers their capabilities, controls container orchestration.
- **ManufacturingOptimization.ProviderSimulator** — technology provider simulator. Emulates manufacturing company behavior.

### Common Libraries

- **ManufacturingOptimization.Common.Messaging** — common library for working with RabbitMQ: interfaces, services, events.
- **ManufacturingOptimization.Common.Models** — common data models: motor specifications, optimization requests, plans, strategies, providers, capabilities.

### Utilities

- **ManufacturingOptimization.Console** — console application for testing and debugging.

## Docker Container Orchestration

The system supports two provider orchestration modes _(User Stories: [US-20](project-overview.md#epic-5-platform-features--extensibility) — Platform extensibility, [US-26](project-overview.md#epic-7-infrastructure--deployment) — Containerization)_:

### Development Mode

**Purpose:** development of provider simulators with debugger support and hot reload.

**Configuration file:** [docker-compose.dev.yml](../docker-compose.dev.yml)

**Orchestrator:** `ComposeManagedOrchestrator` — a stub that only logs actions but does not manage containers.

**Features:**
- All providers are defined as static services in docker-compose
- Full Visual Studio debugger support
- Dynamic addition/removal/pause of providers is **not available**

### Production Mode

**Purpose:** all scenarios except development of the provider simulators themselves.

**Configuration file:** [docker-compose.yml](../docker-compose.yml) for the main stack, provider list from [providers.json](../ManufacturingOptimization.ProviderRegistry/providers.json)

**Orchestrator:** `DockerProviderOrchestrator` — manages the full lifecycle of provider containers through Docker API.

**Features:**
- Providers are created dynamically at system startup
- Supports programmatic addition/removal/pause of providers
- Debugging provider code is **not available** (runs from pre-built images)

### Mode Selection and DI Container Registration

The orchestration mode is set via the `Orchestration__Mode` environment variable in the ProviderRegistry service.

Depending on the mode, the corresponding implementation of the `IProviderOrchestrator` interface is registered in the DI container:
- **Development** → `ComposeManagedOrchestrator`
- **Production** → `DockerProviderOrchestrator`

Orchestrator implementations:
- [IProviderOrchestrator.cs](../ManufacturingOptimization.ProviderRegistry/Abstractions/IProviderOrchestrator.cs) — interface
- [ComposeManagedOrchestrator.cs](../ManufacturingOptimization.ProviderRegistry/Services/ComposeManagedOrchestrator.cs) — Development mode
- [DockerProviderOrchestrator.cs](../ManufacturingOptimization.ProviderRegistry/Services/DockerProviderOrchestrator.cs) — Production mode
- [OrchestrationSettings.cs](../ManufacturingOptimization.ProviderRegistry/Settings/OrchestrationSettings.cs) — settings

### Startup Instructions

**Production Mode:**
1. Build provider image: run script [build-provider-image.ps1](../build-provider-image.ps1)
2. Set `docker-compose` project as startup project in Visual Studio
3. Start (F5)
4. `DockerProviderOrchestrator` creates containers for each provider from the list via Docker API

**Development Mode:**
1. Rename `docker-compose.dev.yml` to `docker-compose.override.yml`
2. Set `docker-compose` project as startup project in Visual Studio
3. Start (F5)
4. Docker Compose starts all services from the static file [docker-compose.override.yml](../docker-compose.dev.yml)