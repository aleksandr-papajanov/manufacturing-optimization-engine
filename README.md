# Manufacturing Optimization Engine

Manufacturing Optimization Engine is a distributed system for optimizing manufacturing processes that automatically analyzes customer requests, selects optimal manufacturing providers, and creates production plans considering technical constraints, deadlines, and costs. The system is built on a microservices architecture using .NET, RabbitMQ for asynchronous messaging, and supports working with multiple providers simultaneously.

> **📖 Detailed project description:** See [Project Overview](docs/project-overview.md) — detailed use case description, Upgrade/Refurbish processes, provider types, workflow steps, and complete project backlog.

## Phase 1 Documentation — Foundation and Optimization

1. [System Architecture](docs/01-architecture.md) — project structure, Docker container orchestration
2. [Messaging Architecture](docs/02-messaging.md) — message interfaces, RabbitMQ, exchanges and routing keys
3. [System Startup Synchronization](docs/03-system-readiness.md) — System Readiness mechanism, microservice startup coordination
4. [Optimization Process (Pipeline)](docs/04-optimization-pipeline.md) — request processing, Workflow Pipeline, optimization steps
5. [Optimization Step](docs/05-optimization-step.md) — strategy generation with Google OR-Tools, linear programming

### Phase 1 Summary

**Status as of January 19-20, 2026**

**According to [Implementation Roadmap](docs/project-overview.md#implementation-roadmap), Phase 1 includes:** US-01, US-06, US-11, US-12, US-13, US-15, US-26

**Phase 1 Goal:** Core loop working — provider registration → customer request → optimization → provider notification. Dockerized deployment.

**Implementation Status:**

**✅ Fully Implemented (7 of 7):**
- **[US-01](docs/project-overview.md#epic-1-technology-provider-management)** — Provider submit capabilities  
  → Provider registration via messaging, ProviderRegisteredEvent
- **[US-06](docs/project-overview.md#epic-2-customer-request-management)** — Customer submit requests  
  → HTTP API in Gateway, publishing RequestOptimizationPlanCommand
- **[US-11](docs/project-overview.md#epic-3-optimization--matchmaking)** — Provider capability validation  
  → ProviderCapabilityValidationService in Engine, validation of capabilities and technical requirements
- **[US-12](docs/project-overview.md#epic-3-optimization--matchmaking)** — Workflow matching  
  → WorkflowMatchingStep determines Upgrade (8 steps) vs Refurbish (5 steps)
- **[US-13](docs/project-overview.md#epic-3-optimization--matchmaking)** — Step assignment optimization  
  → OptimizationStep with Google OR-Tools, 4 strategies with different priorities
- **[US-15](docs/project-overview.md#epic-4-process-coordination--workflow)** — Provider receive proposals  
  → RabbitMQ queues, RPC pattern via ProposeProcessToProviderCommand
- **[US-26](docs/project-overview.md#epic-7-infrastructure--deployment)** — Containerization  
  → Docker, docker-compose.yml, two orchestration modes (Development/Production)

**✨ Additionally Implemented (not part of Phase 1, but ready):**
- **[US-02](docs/project-overview.md#epic-1-technology-provider-management)** — Provider strategies (Upgrade/Refurbish support in ProcessCapabilities)
- **[US-05](docs/project-overview.md#epic-1-technology-provider-management)** — Provider proposal handling (Auto-approval via simulators)
- **[US-07](docs/project-overview.md#epic-2-customer-request-management)** — Strategy recommendations (Multiple strategies, polling, selection)
- **[US-20](docs/project-overview.md#epic-5-platform-features--extensibility)** — Platform extensibility (Microservices architecture, modularity)

**Key Phase 1 Achievements:**
- Microservices architecture with asynchronous messaging
- Docker orchestration with two modes (Development/Production)
- System startup synchronization via System Readiness
- Complete Workflow Pipeline for processing optimization requests
- Generation of 4 strategies with different priorities via Google OR-Tools
- Provider validation and registration via RabbitMQ
- HTTP API Gateway for client requests
- RPC pattern for request-reply interactions