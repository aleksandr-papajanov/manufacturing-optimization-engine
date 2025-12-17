
# Project: Distributed (Re)Manufacturing Engine

## 1. Introduction

Modern manufacturing and remanufacturing ecosystems rely increasingly on **distributed actors**: multiple plants, service providers, logistics operators, and reman centers. Each has different capabilities, costs, and constraints. Finding the **best combination of actors** to execute a manufacturing or remanufacturing process chain is a real-world optimization challenge.

At the same time, industrial systems are moving toward **event-driven architectures**. Plants and systems communicate using **message brokers**, enabling loosely coupled coordination, real-time updates, and scalable automation.

This student project combines both elements:

- **Optimization**: selecting the optimal set of actors to perform a sequence of industrial processes.
- **Event-driven communication**: using a message broker to coordinate requests, responses, plans, and notifications between simulated actors and users.

This mirrors mechanisms used in modern digital manufacturing platforms and provides practical experience with optimization, distributed design, and system integration.


## 2. What Will Be Developed

Students will build a simplified but realistic **optimization and coordination platform** for manufacturing or remanufacturing. These are suggestions on the architecture; the team can decide on the actual choices depending on their ambitions.

### 2.1 Optimization Engine
- Accepts process-chain scenarios (e.g., disassembly → cleaning → machining).
- Retrieves actors and their capabilities.
- Computes the “route” plan based on criteria such as cost, time, or emissions.
- Aggregates LCA data to calculate overall cost
- Outputs a proposed assignment of processes to actors.

### 2.2 Actor Registry
- Stores actor metadata.
- Provides data to the optimization engine through events or simple APIs.

### 2.3 Actor Simulators
- Represent real plants or service providers.
- Subscribe to process proposals.
- Respond with acceptance or rejection of assigned tasks.

### 2.4 Messaging Infrastructure
- A message broker enables asynchronous communication between all components.

### 2.5 User Interface / API
- Simple user-facing interface.
- Let's users create scenarios and view optimization results.
- Receives updates on plan confirmations or failures.



## 3. Scope of the Project (10 Weeks)

This project is designed for a **10-week timeline** and focuses on producing a **functional prototype**.

### In Scope
- A limited but clear manufacturing/remanufacturing domain:
  - 5–10 actors
  - 4–6 process types
- A single or multi-objective optimization system.
- Event-driven communication between services.
- A minimal interface for scenario input and result display.
- Basic actor acknowledgment flow.
- LCA data aggregation.
- **Containerized deployment using Docker or Kubernetes.**

### Good to include (to discuss).
- Security integrated into message broker (encryption, access control)
- Decentralized approach (Zero Knowledge, Local ranking)

## 4. Technologies (General Guidelines but Flexible)

- The project is intentionally flexible. Students may choose their own tools, but the following elements are required:

### 4.1 Optimization
- Any solver or heuristic (LP, MILP, constraint programming, or greedy).
- Example: Pymoo library https://pymoo.org/

### 4.2 Messaging
- Any broker supporting publish/subscribe patterns.
- Suggestion: RabbitMQ https://www.rabbitmq.com/

### 4.3 Services / Components
- Modular services built using any programming languages or frameworks.

### 4.4 UI Layer
- Minimal web dashboard, CLI, or desktop UI.

### 4.5 Storage
- Lightweight storage (JSON files, SQLite, small DB).

### 4.6 **Deployment (Mandatory)**
Students must use **Docker** or **Kubernetes** to deploy their system:

- Each service (optimization engine, actor registry, actors, UI, etc.) must run in its own container.
- The message broker must run as a containerized service.
- Students may choose between:
  - **Docker Compose** (simpler)
  - **Kubernetes** (more advanced; minikube, kind, or cloud cluster)
- Deliverables must include:
  - `Dockerfile` for each component  
  - A `docker-compose.yml` **OR** Kubernetes manifests (`Deployment`, `Service`, etc.)  
  - Documentation explaining how to start the system locally

This requirement ensures experience with modern deployment workflows widely used in industry.


---

## 4. Milestones (10-Week Timeline)

### **Week 1 – Project Setup**
- Understand requirements and domain.  
- Research optimization and event-driven design.  
- Create initial architecture diagram.
- Familiarize with containerization

### **Week 2 – Data Models & Event Schemas**
- Define data structures for actors, processes, and plans.  
- Define event schemas and example payloads.
- Develop a simple system and containerize it.

### **Week 3 – Actor Registry**
- Implement basic registry service and data access.
- Test communication between multiple services using docker
- Publish dummy `actor.updated` events.

### **Week 4 – Initial Optimization Engine**
- Implement basic optimization (manual trigger).  
- Test with static actor data.

### **Week 5 – Integrate Message Broker**
- Connect optimization engine to message broker.  
- Build lightweight actor microservices.
- Test communication between a single actor and a broker

### **Week 6 – Actor Simulators**.  
- Subscribe to plan proposals.  
- Publish acceptance/rejection.

### **Week 7 – Plan Coordination Logic**
- Handle accept events.  
- Determine when to confirm or reject a strategy.  
- Update strategy status accordingly.

### **Week 8 – User Interface / Scenario Input**
- Build a simple interface (web or CLI).  
- Allow creation and submission of scenarios.  
- Display optimization outputs.

### **Week 9 – End-to-End Integration**
- Test flexible deployment of services.  
- Test several scenarios.  
- Fix event ordering or missing/duplicate messages.

### **Week 10 – Finalization**
- Finalize documentation.  
- Polish UI.  
- Prepare final presentation.  
- Deliver full prototype.

---
