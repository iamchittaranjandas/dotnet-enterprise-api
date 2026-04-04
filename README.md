# .NET Enterprise API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

A production-ready, enterprise-grade REST API built with **ASP.NET Core (.NET 10)** following **Clean Architecture**, **SOLID principles**, **CQRS pattern**, and **Domain-Driven Design (DDD)**.

## 📖 Table of Contents

- [Overview](#-overview)
- [Architecture Patterns](#-architecture-patterns)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [Data Provider Selection](#-data-provider-selection)
- [Enterprise Features](#-enterprise-features)
- [AI Agent (Microsoft Agent Framework)](#-ai-agent-microsoft-agent-framework-net-10)
- [RAG (Retrieval-Augmented Generation)](#-rag-retrieval-augmented-generation)
- [Multi-Provider AI Support](#-multi-provider-ai-support)
- [pgvector Store](#-pgvector-store)
- [Hybrid Search](#-hybrid-search)
- [Multi-Agent Workflow](#-multi-agent-workflow)
- [AI Workflow Automation Engine](#-ai-workflow-automation-engine)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [CQRS Implementation](#-cqrs-implementation)
- [SOLID Principles](#-solid-principles)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

This project serves as a **comprehensive template** for building enterprise-grade APIs with modern .NET development practices. It demonstrates how to build a secure, maintainable, and highly scalable REST API using industry-standard patterns and principles

## 🏗️ Architecture Patterns

- **Clean Architecture** - Separation of concerns with clear boundaries
- **CQRS (Command Query Responsibility Segregation)** - Separate read and write operations
- **MediatR** - Mediator pattern for decoupled request/response handling
- **SOLID Principles** - Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Domain-Driven Design** - Rich domain models with domain events
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Result Pattern** - Standardized response handling

## 🚀 Key Features

- **CQRS with MediatR** - Commands and Queries separation
- **FluentValidation** - Automatic request validation pipeline
- **Pipeline Behaviors** - Logging, Performance monitoring, Validation
- **Domain Events** - Event-driven architecture support
- **JWT Authentication** - Secure token-based authentication
- **Role-Based Authorization** - Fine-grained access control
- **Global Exception Handling** - Centralized error management
- **Request Logging Middleware** - Comprehensive request/response logging
- **Swagger/OpenAPI** - Interactive API documentation
- **Multi-Database Support** - Switch between SQL Server, PostgreSQL, MySQL, or Oracle with a single config change
- **Multi-Provider Data Access** - Choose between Entity Framework, Dapper, or ADO.NET via config
- **Health Checks** - Database and API health monitoring at `/health`
- **Rate Limiting** - Fixed window, sliding window, and token bucket policies
- **API Versioning** - Query string and header-based version control
- **OpenTelemetry** - Distributed tracing and metrics instrumentation
- **Output Caching** - Response caching with tag-based invalidation
- **Redis Caching** - Optional Redis-backed distributed cache and output cache (falls back to in-memory)
- **Refresh Tokens** - Secure token rotation for long-lived sessions
- **Cursor-Based Pagination** - Efficient cursor pagination with `cursor` and `pageSize` query parameters
- **AutoMapper** - Centralized object mapping with mapping profiles
- **Extension Methods** - Clean `Program.cs` with service and middleware extension methods
- **AI Agent (Microsoft Agent Framework 1.0)** - Natural-language task management via `POST /api/agent/chat` with tool-calling to list, create, update, and delete tasks
- **RAG (Retrieval-Augmented Generation)** - Semantic similarity search over task embeddings injected into every agent prompt; embeddings auto-generated on task create/update and rehydrated at startup; embedding cache via Redis/memory; metadata filtering; full observability logging with similarity scores
- **Multi-Provider AI** - Switch between OpenAI, Claude, Gemini, Groq, OpenRouter, NVIDIA NIM, or xAI Grok via a single `AI:Provider` config key; no code changes required
- **pgvector Store** - PostgreSQL + pgvector persisted vector store; auto-activated when `DatabaseProvider=PostgreSQL`; survives restarts without rehydration cost
- **Hybrid Search** - BM25 keyword scoring + cosine vector similarity fused via Reciprocal Rank Fusion (RRF); delivers higher precision than pure vector search; on by default
- **Multi-Agent Workflow** - Orchestrator classifies intent (query / mutation / both) and routes to specialised QueryAgent or MutationAgent; `POST /api/multiagent/chat`
- **AI Workflow Automation Engine** - Step-based LLM pipeline with Llm / Tool / Condition step types; three built-in workflows (triage, daily-briefing, auto-close); prompt template variables `{input}` and `{context}`; execution history polled by ID via `GET /api/workflow/{executionId}`

---

## 📦 Technologies & Packages

- **ASP.NET Core Web API** (.NET 10)
- **MediatR** (12.4.1) - CQRS implementation
- **FluentValidation** (11.11.0) - Request validation
- **Entity Framework Core** (10.0.5) - Full ORM with change tracking & migrations
- **Dapper** (2.1.72) - Lightweight micro-ORM with raw SQL
- **ADO.NET** - Pure data access, no ORM
- **SQL Server** / **PostgreSQL** / **MySQL** / **Oracle** - Multi-database support via config
- **Npgsql** (10.0.2) - PostgreSQL data provider
- **MySqlConnector** (2.5.0) - MySQL data provider
- **Oracle.ManagedDataAccess.Core** (23.26.100) - Oracle data provider
- **JWT Bearer Authentication** + **Refresh Tokens**
- **BCrypt.Net** - Secure password hashing
- **AutoMapper** (16.1.1) - Object mapping with profiles
- **Swagger/OpenAPI** - API documentation
- **OpenTelemetry** (1.15.0) - Distributed tracing & metrics
- **Asp.Versioning** (8.1.1) - API version management
- **Health Checks** - Database-specific health monitoring (SQL Server, PostgreSQL, MySQL, Oracle, Redis)
- **StackExchange.Redis** - Distributed caching and output cache backing
- **Microsoft.Extensions.Caching.StackExchangeRedis** (10.0.5) - IDistributedCache with Redis
- **Microsoft.AspNetCore.OutputCaching.StackExchangeRedis** (10.0.5) - Output cache Redis store
- **Microsoft.Agents.AI** (1.0.0) - Microsoft Agent Framework for building LLM-powered agents
- **Microsoft.Extensions.AI** (10.4.1) - Provider-agnostic AI abstractions (`IChatClient`, `IEmbeddingGenerator`, `Embedding<float>`)
- **Microsoft.Extensions.AI.OpenAI** (10.4.1) - OpenAI & OpenAI-compatible provider adapters (Groq, OpenRouter, NVIDIA NIM, xAI Grok)
- **Anthropic** (12.11.0) - Official Anthropic Claude SDK with `IChatClient` adapter
- **Google.GenAI** (1.6.1) - Official Google Gemini SDK with `IChatClient` adapter
- **Microsoft.Extensions.VectorData.Abstractions** (10.1.0) - Vector store abstraction layer (swappable to Azure AI Search, Qdrant, etc.)
- **Pgvector.EntityFrameworkCore** (0.3.0) - pgvector EF Core integration: `vector(1536)` column type, `CosineDistance` LINQ operator, `UseVector()` Npgsql extension

---

## 🏛️ Clean Architecture Structure

### 1. **API Layer** (`DotnetEnterpriseApi.Api`)
**Responsibilities:**
- HTTP request/response handling
- Controllers (thin, delegating to MediatR)
- Middleware (Exception handling, Request logging)
- Extension methods for clean service registration and middleware pipeline
- Authentication & Authorization setup
- Swagger configuration

**Key Files:**
- `Controllers/` - API endpoints using MediatR
- `Extensions/` - Service registration and middleware pipeline extension methods
- `Middleware/` - Custom middleware components
- `Program.cs` - Clean application startup using extension methods

### 2. **Application Layer** (`DotnetEnterpriseApi.Application`)
**Responsibilities:**
- CQRS Commands and Queries
- Command/Query Handlers
- Validators (FluentValidation)
- Pipeline Behaviors
- Application interfaces
- DTOs and Response models
- Business logic orchestration

**Structure:**
```
Application/
├── Common/
│   ├── Behaviours/          # MediatR pipeline behaviors
│   ├── Exceptions/          # Custom exceptions
│   ├── Interfaces/          # Application contracts
│   ├── Mappings/            # AutoMapper profiles
│   └── Models/              # Result pattern, cursor pagination models
├── Features/
│   ├── Authentication/
│   │   ├── Commands/        # Register, Login commands
│   │   └── EventHandlers/   # Domain event handlers
│   └── Tasks/
│       ├── AgentTools/      # AI agent tool methods (Microsoft Agent Framework)
│       ├── Commands/        # Create, Update, Delete commands
│       ├── EventHandlers/   # Task-related event handlers + RAG embedding handler
│       ├── Models/          # TaskVectorRecord, RagRetrievalResult — embedding records & retrieval results
│       └── Queries/         # GetAll, GetById queries
└── DependencyInjection.cs   # Application services registration
```

### 3. **Domain Layer** (`DotnetEnterpriseApi.Domain`)
**Responsibilities:**
- Core business entities
- Domain events
- Business rules and invariants
- Value objects
- Domain interfaces

**Structure:**
```
Domain/
├── Common/
│   ├── BaseEntity.cs           # Base entity with domain events
│   ├── BaseAuditableEntity.cs  # Auditable entity base
│   └── IDomainEvent.cs         # Domain event interface
├── Entities/
│   ├── AppUser.cs
│   └── TaskItem.cs
└── Events/
    ├── UserRegisteredEvent.cs
    └── TaskCreatedEvent.cs
```

### 4. **Infrastructure Layer** (`DotnetEnterpriseApi.Infrastructure`)
**Responsibilities:**
- Database context implementation
- Repository implementations (EF Core, Dapper, ADO.NET)
- Unit of Work implementation
- Database and data provider selection via configuration
- SQL dialect abstraction for multi-database support
- Data persistence

**Structure:**
```
Infrastructure/
├── Data/
│   ├── AppDbContext.cs              # EF Core DbContext with domain events
│   ├── DatabaseConnectionFactory.cs # Multi-database connection factory
│   └── Dialects/
│       ├── SqlServerDialect.cs      # SQL Server SQL generation
│       ├── PostgreSqlDialect.cs     # PostgreSQL SQL generation
│       ├── MySqlDialect.cs          # MySQL SQL generation
│       └── OracleDialect.cs         # Oracle SQL generation
├── Persistence/
│   ├── UnitOfWork.cs              # EF Core transaction management
│   └── DapperUnitOfWork.cs        # Dapper/ADO transaction management
├── Repositories/
│   ├── EntityFramework/
│   │   ├── EfTaskRepository.cs    # EF Core implementation
│   │   └── EfUserRepository.cs
│   ├── Dapper/
│   │   ├── DapperTaskRepository.cs # Dapper implementation
│   │   └── DapperUserRepository.cs
│   └── Ado/
│       ├── AdoTaskRepository.cs    # Pure ADO.NET implementation
│       └── AdoUserRepository.cs
├── Migrations/
└── DependencyInjection.cs         # Provider selection & DI registration
```

### 🔄 Dependency Flow

```
API Layer → Application Layer → Domain Layer
Infrastructure Layer → Application Layer + Domain Layer
```

**Key Principle:** Dependencies point inward. The Domain layer has no dependencies. The Application layer depends only on Domain. Infrastructure implements Application interfaces.

---

## 🗄️ Database Provider Selection

This project supports **four database engines**. Switch with a single config change — no code modifications required.

### How to Switch

Edit `DotnetEnterpriseApi.Api/appsettings.json`:
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DotNetEnterpriseApiDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

| Value | Connection String Example | Best For |
|-------|--------------------------|----------|
| `SqlServer` | `Server=(localdb)\\mssqllocaldb;Database=MyDb;Trusted_Connection=True;` | Production, enterprise workloads |
| `PostgreSQL` | `Host=localhost;Database=MyDb;Username=postgres;Password=pass` | Open-source production deployments |
| `MySQL` | `Server=localhost;Database=MyDb;User=root;Password=pass;` | Web applications, cross-platform deployments |
| `Oracle` | `Data Source=localhost:1521/XEPDB1;User Id=system;Password=pass;` | Enterprise, legacy system integration |

### How It Works

The `DatabaseProvider` setting controls:

1. **EF Core Provider** — switches between `UseSqlServer()`, `UseNpgsql()`, `UseMySql()`, `UseOracle()`
2. **Connection Factory** — creates `SqlConnection`, `NpgsqlConnection`, `MySqlConnection`, or `OracleConnection`
3. **SQL Dialect** — generates database-specific SQL (e.g., `OUTPUT INSERTED.Id` vs `RETURNING Id` vs `LAST_INSERT_ID()` vs `RETURNING INTO`)
4. **Health Checks** — registers the appropriate database health check

> **Note:** When switching database providers with EF Core, delete existing migrations and regenerate with `dotnet ef migrations add InitialCreate`, or use `Database.EnsureCreated()` for development.

---

## 🔌 Data Provider Selection

This project supports **three data access strategies**. You choose which one to use by setting a single value in `appsettings.json` — no code changes required.

### How to Switch

Edit `DotnetEnterpriseApi.Api/appsettings.json`:
```json
{
  "DataProvider": "EntityFramework"
}
```

| Value | Description | Best For |
|-------|-------------|----------|
| `EntityFramework` | EF Core with DbContext, change tracking, LINQ, migrations | Rapid development, complex queries, automatic schema management |
| `Dapper` | Micro-ORM with raw SQL and automatic object mapping | High-performance reads, fine-tuned SQL control |
| `Ado` | Pure ADO.NET with DbCommand/DbDataReader, no ORM | Maximum control, zero abstraction overhead |

### How It Works

```
appsettings.json ["DataProvider"] + ["DatabaseProvider"]
        │
        ▼
DependencyInjection.cs (Infrastructure)
        │
        ├── "EntityFramework"
        │       ├── AppDbContext (UseSqlServer / UseNpgsql / UseMySql / UseOracle)
        │       ├── UnitOfWork (EF transactions)
        │       ├── EfTaskRepository
        │       └── EfUserRepository
        │
        ├── "Dapper"
        │       ├── DatabaseConnectionFactory (SqlConnection / NpgsqlConnection / MySqlConnection / OracleConnection)
        │       ├── ISqlDialect (SqlServer / PostgreSQL / MySQL / Oracle)
        │       ├── DapperUnitOfWork
        │       ├── DapperTaskRepository
        │       └── DapperUserRepository
        │
        └── "Ado"
                ├── DatabaseConnectionFactory (SqlConnection / NpgsqlConnection / MySqlConnection / OracleConnection)
                ├── ISqlDialect (SqlServer / PostgreSQL / MySQL / Oracle)
                ├── DapperUnitOfWork
                ├── AdoTaskRepository
                └── AdoUserRepository
```

All three providers implement the **same interfaces** (`ITaskRepository`, `IUserRepository`, `IUnitOfWork`), so the Application and API layers remain unchanged regardless of which provider is active.

### Request Flow (Same for All Providers)

```
HTTP Request
  │
  ▼
Controller (TasksController / AuthController)
  │
  ▼
MediatR.Send(Command or Query)
  │
  ▼
Handler (uses ITaskRepository / IUserRepository)
  │
  ▼
Repository Implementation (EF / Dapper / ADO — selected at startup)
  │
  ▼
Database (SQL Server / PostgreSQL / MySQL / Oracle — selected at startup)
  │
  ▼
Result<T> flows back up through the same chain
  │
  ▼
HTTP Response (JSON)
```

### Provider Comparison

| Feature | EntityFramework | Dapper | ADO.NET |
|---------|:-:|:-:|:-:|
| Change Tracking | Yes | No | No |
| LINQ Support | Yes | No | No |
| Raw SQL | Optional | Yes | Yes |
| Object Mapping | Automatic | Automatic | Manual |
| Migrations | Yes | No | No |
| Performance | Good | Excellent | Excellent |
| Code Verbosity | Low | Medium | High |
| Learning Curve | Medium | Low | Low |

### Example: Same Operation, Three Implementations

**Get Task by ID**

**Entity Framework:**
```csharp
return await _context.Tasks.FindAsync(id);
```

**Dapper:**
```csharp
const string sql = "SELECT Id, Title, Description, IsCompleted, CreatedDate FROM Tasks WHERE Id = @Id";
using var connection = _connectionFactory.CreateConnection();
return await connection.QueryFirstOrDefaultAsync<TaskItem>(sql, new { Id = id });
```

**ADO.NET:**
```csharp
const string sql = "SELECT Id, Title, Description, IsCompleted, CreatedDate FROM Tasks WHERE Id = @Id";
using var connection = (SqlConnection)_connectionFactory.CreateConnection();
await connection.OpenAsync();
using var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@Id", id);
using var reader = await command.ExecuteReaderAsync();
return await reader.ReadAsync() ? MapTaskItem(reader) : null;
```

---

## 🏢 Enterprise Features

All features below are **built-in and ready to use** out of the box. No extra setup required.

### Health Checks

Endpoint: `GET /health`

Returns JSON with API and database health status:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "sqlserver",
      "status": "Healthy",
      "duration": "184ms"
    },
    {
      "name": "redis",
      "status": "Healthy",
      "duration": "12ms"
    }
  ],
  "totalDuration": "188ms"
}
```

### Rate Limiting

Protects your API from abuse. Three built-in policies:

| Policy | Type | Limit | Window |
|--------|------|-------|--------|
| `fixed` | Fixed Window | 100 requests | 1 minute |
| `sliding` | Sliding Window | 50 requests | 1 minute |
| `token` | Token Bucket | 100 tokens, 10/10s refill | Continuous |

A **global limiter** (200 req/min per user/IP) is applied to all endpoints automatically. When the limit is exceeded, the API returns `429 Too Many Requests`.

### API Versioning

Supports two methods simultaneously:

```
# Query string
GET /api/tasks?api-version=1.0

# Header
GET /api/tasks
X-Api-Version: 1.0
```

Response headers include `api-supported-versions` to tell clients which versions are available.

### OpenTelemetry Observability

Built-in distributed tracing and metrics:

- **Traces** - Every HTTP request is traced with span context
- **Metrics** - ASP.NET Core and HTTP client metrics collected automatically
- **Console Exporter** - Outputs to console by default (replace with Jaeger, Zipkin, OTLP, or Application Insights for production)

To switch to a production exporter, replace `AddConsoleExporter()` in `Program.cs` with your preferred exporter package.

### Redis Caching (Optional)

Redis support is **auto-enabled** when a Redis connection string is configured. When not configured, the app uses in-memory caching — no code changes needed.

**Configuration in `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Redis": {
    "InstanceName": "DotnetEnterpriseApi:"
  }
}
```

| `ConnectionStrings:Redis` | Behavior |
|---|---|
| Empty `""` or missing | In-memory cache (default, no Redis needed) |
| `"localhost:6379"` | Redis for distributed cache + output cache |
| `"your-cache.redis.cache.windows.net:6380,password=key,ssl=True"` | Azure Redis Cache |

**What Redis enables:**

| Feature | Without Redis | With Redis |
|---------|---|---|
| `IDistributedCache` | In-memory (per instance) | Shared across instances |
| Output Cache | In-memory (lost on restart) | Persists across restarts |
| Health Check `/health` | DB only | DB + Redis |

**Usage — inject `IDistributedCache` anywhere:**
```csharp
public class MyHandler
{
    private readonly IDistributedCache _cache;

    public MyHandler(IDistributedCache cache) => _cache = cache;

    public async Task Handle()
    {
        // Set cache
        await _cache.SetStringAsync("key", "value", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        // Get cache
        var value = await _cache.GetStringAsync("key");
    }
}
```

### Output Caching

Response caching reduces database load:

| Policy | TTL | Description |
|--------|-----|-------------|
| Default | 30 seconds | Applied to all endpoints |
| `tasks` | 60 seconds | Tag-based, applied to task listing |

Cache is automatically invalidated when data changes. When Redis is configured, output cache is backed by Redis and survives app restarts.

### Refresh Tokens

Login now returns both a JWT access token and a refresh token:

```json
{
  "token": "eyJhbG...",
  "refreshToken": "FyuFem...",
  "userName": "john",
  "email": "john@example.com",
  "role": "User"
}
```

Use `POST /api/auth/refresh-token` with the refresh token to get new tokens without re-entering credentials. Refresh tokens expire in 7 days and are single-use (revoked after each rotation).

### Cursor-Based Pagination

All list endpoints use cursor-based pagination for consistent, performant scrolling through large datasets:

```
# First page
GET /api/tasks?pageSize=10

# Next page (use nextCursor from previous response)
GET /api/tasks?cursor=5&pageSize=10
```

| Parameter | Default | Description |
|-----------|---------|-------------|
| `cursor` | `null` | ID of the last item from the previous page (omit for first page) |
| `pageSize` | 10 | Items per page |

**Response:**
```json
{
  "items": [...],
  "nextCursor": 5,
  "hasNextPage": true
}
```

**Why cursor-based?** Unlike offset pagination (`OFFSET...FETCH`), cursor pagination avoids skipping rows when data is inserted or deleted between requests, and performs consistently regardless of page depth.

---

## 🎯 SOLID Principles Implementation

### **Single Responsibility Principle (SRP)**
- Each handler handles one command/query
- Separate validators for each request
- Dedicated event handlers for domain events

### **Open/Closed Principle (OCP)**
- Pipeline behaviors extend functionality without modifying existing code
- Base classes allow extension through inheritance

### **Liskov Substitution Principle (LSP)**
- Interface-based design allows substitution
- Repository pattern enables different implementations

### **Interface Segregation Principle (ISP)**
- Small, focused interfaces (IApplicationDbContext, IUnitOfWork)
- Clients depend only on interfaces they use

### **Dependency Inversion Principle (DIP)**
- High-level modules depend on abstractions
- Dependency injection throughout
- Infrastructure implements Application interfaces

---

---

## 📋 CQRS Pattern Implementation

### Commands (Write Operations)
Commands modify state and return a Result object:

**Example: Register User Command**
```csharp
// Command
public class RegisterCommand : IRequest<Result<RegisterResponse>>
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

// Handler
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    // Implementation with validation, business logic, and persistence
}

// Validator
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    // FluentValidation rules
}
```

### Queries (Read Operations)
Queries retrieve data without modifying state:

**Example: Get All Tasks Query**
```csharp
// Query
public class GetAllTasksQuery : IRequest<Result<List<TaskResponse>>>
{
}

// Handler
public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, Result<List<TaskResponse>>>
{
    // Implementation to retrieve and return data
}
```

### Pipeline Behaviors
Automatic cross-cutting concerns applied to all requests:

1. **ValidationBehaviour** - Validates requests using FluentValidation
2. **LoggingBehaviour** - Logs request execution
3. **PerformanceBehaviour** - Monitors long-running requests (>500ms)

---

## 🔐 Authentication Flow

1. User registers with email and password
2. Password is securely hashed using BCrypt
3. User logs in using credentials
4. API generates a JWT token with claims
5. Client sends JWT token in Authorization header
6. Protected endpoints validate the token
7. Role-based authorization controls access

**Example Authorization Header:**
```
Authorization: Bearer {your_jwt_token}
```

---

## 🌐 API Endpoints

### Authentication Endpoints

**POST** `/api/auth/register`
- Register a new user
- Request body: `{ "userName": "string", "email": "string", "password": "string" }`
- Response: `Result<RegisterResponse>`

**POST** `/api/auth/login`
- Login and receive JWT token
- Request body: `{ "email": "string", "password": "string" }`
- Response: `Result<LoginResponse>` with JWT + Refresh Token

**POST** `/api/auth/refresh-token`
- Exchange a refresh token for new JWT + refresh token pair
- Request body: `{ "token": "string" }`
- Response: `Result<LoginResponse>` with new tokens

### Health & Monitoring Endpoints

**GET** `/health`
- Returns API and database health status (no auth required)

### Task Management Endpoints (Requires Authentication)

**GET** `/api/tasks?cursor={id}&pageSize=10`
- Get all tasks with cursor-based pagination
- Response: `Result<CursorPagedResult<TaskResponse>>`

**GET** `/api/tasks/{id}`
- Get task by ID
- Response: `Result<TaskResponse>`

**POST** `/api/tasks`
- Create new task
- Request body: `{ "title": "string", "description": "string" }`
- Response: `Result<TaskResponse>`

**PUT** `/api/tasks/{id}`
- Update task
- Request body: `{ "title": "string", "description": "string", "isCompleted": boolean }`
- Response: `Result<TaskResponse>`

**DELETE** `/api/tasks/{id}` (Admin only)
- Delete task
- Response: `Result`

### AI Agent Endpoints (Requires Authentication)

**POST** `/api/agent/chat`
- RAG-augmented single agent; hybrid search on by default
- Request: `{ "message": "string", "filterCompleted": bool|null, "hybrid": bool }`
- Response: `{ "reply": "string", "ragContext": [...] }`

**POST** `/api/multiagent/chat`
- Orchestrator classifies intent → routes to QueryAgent or MutationAgent
- Request: `{ "message": "string", "filterCompleted": bool|null }`
- Response: `{ "reply": "string", "agentUsed": "string", "intent": "string", "ragContext": [...] }`

---

## 🤖 AI Agent (Microsoft Agent Framework .NET 1.0)

### Overview

The API includes a conversational AI agent powered by [Microsoft Agent Framework .NET 1.0](https://devblogs.microsoft.com/agent-framework/microsoft-agent-framework-version-1-0/). The agent understands natural language and manages tasks on your behalf by calling the appropriate tool. It works with any of the [supported AI providers](#-multi-provider-ai-support) — switch with a single config key.

```
User: "Create a task called 'Fix login bug' with description 'Fails on Safari'"
Agent: "Task created successfully. ID: 7, Title: 'Fix login bug'"

User: "List my pending tasks"
Agent: "[ID:5] Deploy to prod — Pending | Deploy the updated API to production
         [ID:7] Fix login bug — Pending | Fails on Safari"
```

### Architecture

```
POST /api/agent/chat
        │
        ▼
AgentController
        │  builds IChatClient.AsAIAgent() with 5 tools per request
        ▼
ChatClientAgent (Microsoft.Agents.AI)
        │  calls LLM with tool definitions
        ▼
AI Provider (OpenAI / Claude / Gemini / Groq / … — whichever AI:Provider is active)
        │  selects and invokes tool(s)
        ▼
TaskAgentTools (Application Layer)
        │  delegates to ITaskRepository + IUnitOfWork
        ▼
Repository (EF / Dapper / ADO — whichever DataProvider is active)
        │
        ▼
Database
```

### Available Agent Tools

| Tool | Description |
|------|-------------|
| `ListTasksAsync` | Lists tasks with optional cursor-based pagination |
| `GetTaskByIdAsync` | Retrieves a single task by its ID |
| `CreateTaskAsync` | Creates a new task with title and description |
| `UpdateTaskCompletionAsync` | Marks a task as completed or incomplete |
| `DeleteTaskAsync` | Permanently deletes a task by ID |

All tools are registered with `[Description]` attributes so the LLM knows when and how to invoke each one. Tools are wired up using `AIFunctionFactory.Create(...)` from `Microsoft.Extensions.AI`.

### Configuration

Set the active provider via `AI:Provider` (default: `openai`), then supply the API key for that provider. See the [Multi-Provider AI Support](#-multi-provider-ai-support) section for all options.

**Example — OpenAI via user secrets:**
```bash
cd DotnetEnterpriseApi.Api
dotnet user-secrets set "AI:Provider" "openai"
dotnet user-secrets set "AI:OpenAI:ApiKey" "sk-..."
dotnet user-secrets set "AI:OpenAI:ModelId" "gpt-4o-mini"
```

**Example — Claude via environment variables:**
```bash
AI__Provider=claude
AI__Claude__ApiKey=sk-ant-...
AI__Claude__ModelId=claude-sonnet-4-6
```

### Usage Examples

```http
POST /api/agent/chat
Authorization: Bearer {token}
Content-Type: application/json

{ "message": "Show me all my tasks" }
```

**Response:**
```json
{
  "reply": "[ID:1] Setup CI/CD — Pending | Configure GitHub Actions pipeline\n[ID:2] Write tests — Completed | Add unit tests for auth module"
}
```

```http
POST /api/agent/chat
Authorization: Bearer {token}
Content-Type: application/json

{ "message": "Mark task 1 as completed" }
```

**Response:**
```json
{
  "reply": "Task ID 1 marked as completed."
}
```

```http
POST /api/agent/chat
Authorization: Bearer {token}
Content-Type: application/json

{ "message": "Delete task 3" }
```

**Response:**
```json
{
  "reply": "Task ID 3 has been deleted."
}
```

### Key Files

| File | Layer | Purpose |
|------|-------|---------|
| [AgentController.cs](DotnetEnterpriseApi.Api/Controllers/AgentController.cs) | API | HTTP endpoint, agent construction per request |
| [TaskAgentTools.cs](DotnetEnterpriseApi.Application/Features/Tasks/AgentTools/TaskAgentTools.cs) | Application | Tool methods called by the LLM |
| `ServiceCollectionExtensions.AddAgentServices()` | API | Registers `IChatClient` singleton |
| `DependencyInjection.AddApplication()` | Application | Registers `TaskAgentTools` as scoped |

### Switching AI Provider

Set `AI:Provider` in `appsettings.json` or via user secrets / environment variables. No code changes required. See the [Multi-Provider AI Support](#-multi-provider-ai-support) section for all supported providers and their config keys.

---

## 🔍 RAG (Retrieval-Augmented Generation)

### Overview

Every call to `POST /api/agent/chat` is RAG-augmented. Before the LLM is invoked, the user's message is embedded and compared against all stored task embeddings. The top-5 semantically similar tasks are injected into the agent's system instructions, giving the model grounded context to reason from — without extra tool calls.

```
User: "anything related to authentication?"
  ↓
Check IDistributedCache for cached query embedding (Redis / memory)
  ↓
Generate embedding via IEmbeddingGenerator if cache miss → cache result
  ↓
Apply metadata filter (all / completed / pending)
  ↓
Cosine similarity over in-memory vector store
  ↓
Top matches: "Fix JWT expiry bug", "Add refresh token rotation", "OAuth integration"
  ↓ (structured log: taskId, title, score for each match)
Injected into agent instructions as context
  ↓
LLM answers with grounded, relevant response
```

RAG **complements** tool-calling — tools handle create/update/delete mutations; RAG handles semantic read context.

### Architecture

```
App startup
        │
        ▼
RagRehydrationService (IHostedService)
        │  Loads all tasks from DB → embeds each → fills vector store
        ▼
Vector store ready before first request

POST /api/agent/chat  (single agent)
POST /api/multiagent/chat  (orchestrated)
        │
        ▼
Controller
        │  1. RetrieveContextAsync(userMessage, topK: 5, filterCompleted?, hybrid=true)
        ▼
TaskRagService / PgVectorRagService
        │  Cache check → embed query → metadata filter
        │  → vector rank + BM25 keyword rank → RRF fusion → log scores
        ▼
RagRetrievalResult  { Context: string, Matches: [{ taskId, title, score }] }
        │  2. Build agent(s) with tools + enriched instructions
        ▼
LLM (reasons over context + calls tools as needed — whichever AI:Provider is active)


POST /api/tasks  (create)
PUT  /api/tasks/{id}  (update)
        │
        ▼
Command Handler → SaveChanges → Publish TaskCreatedEvent / TaskUpdatedEvent
        │
        ▼
TaskEmbeddingHandler (MediatR notification handler)
        │  Embed title + description → cache embedding → upsert vector record
        ▼
Vector store updated (ready for next search)
```

### Embedding Trigger

Embeddings are automatically generated on three occasions — no manual step required:

| Trigger | When | Action |
|---|---|---|
| Startup rehydration | Application start | All existing tasks embedded from DB |
| `TaskCreatedEvent` | After `POST /api/tasks` saves | New task embedded + inserted |
| `TaskUpdatedEvent` | After `PUT /api/tasks/{id}` saves | Existing record re-embedded + replaced |

The `TaskEmbeddingHandler` catches and logs any embedding failures without propagating them — a broken embedding call never fails the original task request.

### Startup Rehydration

`RagRehydrationService` implements `IHostedService` and runs **once on startup**. It fetches all tasks from `ITaskRepository` (using `IServiceScopeFactory` to safely resolve the scoped repository from a singleton) and calls `UpsertTaskEmbeddingAsync` for each. Individual failures are warned and skipped — the host never crashes.

```
Application starts
        ↓
RagRehydrationService.StartAsync()
        ↓
Load all tasks from database (up to 10,000)
        ↓
For each task: generate embedding → cache → insert into vector store
        ↓
Log: "RAG: Rehydration complete — 42 embedded, 0 failed"
        ↓
Vector store warm and ready for requests
```

Without this, the vector store would be empty after every restart until tasks were individually created or updated.

### Embedding Cache

Every embedding — whether for a task or a user query — is cached in `IDistributedCache`:

- **Key**: `rag:emb:{SHA256(input)}` — same text always hits the same cache slot
- **TTL**: 24 hours
- **Backend**: Redis when `ConnectionStrings:Redis` is set; in-memory otherwise (same as the distributed cache used by the rest of the API)

This means repeated user queries cost zero embedding API calls, and rehydrating 1,000 tasks on restart only calls the embedding API for tasks whose embeddings have expired from cache.

### Metadata Filtering

`RetrieveContextAsync` accepts an optional `filterCompleted: bool?` parameter:

| Value | Effect |
|---|---|
| `null` (default) | All tasks considered |
| `false` | Only pending tasks considered |
| `true` | Only completed tasks considered |

Filtering is applied **before** cosine scoring — irrelevant records are never ranked. Clients can pass `filterCompleted` in the request body:

```json
{ "message": "What auth tasks are still open?", "filterCompleted": false }
```

### Observability

Every retrieval is logged with structured data — no guessing what context was injected:

```
INFO  RAG: Retrieved 3 context item(s) for query 'anything about auth' — top score: 0.8921
DEBUG RAG:   [7] 'Fix JWT expiry bug' (Pending) score=0.8921
DEBUG RAG:   [3] 'Add refresh token rotation' (Completed) score=0.8104
DEBUG RAG:   [12] 'OAuth integration' (Pending) score=0.7633
```

The API response also returns the RAG context as structured JSON so you can inspect it directly from Swagger or Postman without needing server logs:

```json
{
  "reply": "There are 2 open auth-related tasks: ...",
  "ragContext": [
    { "taskId": 7,  "title": "Fix JWT expiry bug",        "isCompleted": false, "score": 0.8921 },
    { "taskId": 3,  "title": "Add refresh token rotation", "isCompleted": true,  "score": 0.8104 },
    { "taskId": 12, "title": "OAuth integration",          "isCompleted": false, "score": 0.7633 }
  ]
}
```

### Vector Store

The current implementation uses an **in-memory `ConcurrentDictionary`** with **cosine similarity** computed in C#. This is intentionally simple and swappable — implement `ITaskRagService` to adopt any vector database:

| Store | Status | How to activate |
|---|---|---|
| In-memory `ConcurrentDictionary` | ✅ Built-in (default) | Any `DatabaseProvider` other than PostgreSQL |
| PostgreSQL + pgvector | ✅ Built-in | Set `DatabaseProvider = PostgreSQL` + `DataProvider = EntityFramework` |
| Redis Vector | 🔌 Implement `ITaskRagService` | Backed by `NRedisStack` vector search |
| Azure AI Search | 🔌 Implement `ITaskRagService` | Backed by `SearchClient` |
| Qdrant | 🔌 Implement `ITaskRagService` | Backed by Qdrant HTTP/gRPC client |

The rest of the codebase (`AgentController`, `MultiAgentController`, `TaskEmbeddingHandler`, `RagRehydrationService`) is unchanged regardless of which store is active.

### Configuration

Embeddings use the same provider as the agent. For providers that don't natively support embeddings (Claude, Gemini, Groq, Grok), the factory automatically falls back to OpenAI embeddings — so `AI:OpenAI:ApiKey` and `AI:OpenAI:EmbeddingModelId` must also be set in that case.

**OpenAI (default):**
```bash
dotnet user-secrets set "AI:OpenAI:EmbeddingModelId" "text-embedding-3-small"
```

**NVIDIA NIM (native embeddings):**
```bash
dotnet user-secrets set "AI:Provider" "nvidia"
dotnet user-secrets set "AI:Nvidia:ApiKey" "nvapi-..."
dotnet user-secrets set "AI:Nvidia:EmbeddingModelId" "nvidia/nv-embedqa-e5-v5"
```

> `text-embedding-3-small` produces 1536-dimensional vectors at low cost. Switch to `text-embedding-3-large` (3072 dims) for higher accuracy — no code changes required, just update the config key.

### Key Files

| File | Layer | Purpose |
|---|---|---|
| [ITaskRagService.cs](DotnetEnterpriseApi.Application/Interfaces/ITaskRagService.cs) | Application | Contract: `UpsertTaskEmbeddingAsync` + `RetrieveContextAsync` (with filter) |
| [RagRetrievalResult.cs](DotnetEnterpriseApi.Application/Features/Tasks/Models/RagRetrievalResult.cs) | Application | Result type: `Context` string + typed `Matches` with similarity scores |
| [TaskVectorRecord.cs](DotnetEnterpriseApi.Application/Features/Tasks/Models/TaskVectorRecord.cs) | Application | In-memory record: TaskId, Title, Description, IsCompleted, Embedding |
| [TaskRagService.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/TaskRagService.cs) | Infrastructure | Vector store: embedding cache + metadata filter + cosine similarity + observability logging |
| [RagRehydrationService.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/RagRehydrationService.cs) | Infrastructure | `IHostedService` — seeds vector store from DB on every app startup |
| [TaskEmbeddingHandler.cs](DotnetEnterpriseApi.Application/Features/Tasks/EventHandlers/TaskEmbeddingHandler.cs) | Application | MediatR handler for `TaskCreatedEvent` and `TaskUpdatedEvent` |
| [TaskUpdatedEvent.cs](DotnetEnterpriseApi.Domain/Events/TaskUpdatedEvent.cs) | Domain | Carries full task data (title, description, isCompleted) for re-embedding |
| [AgentController.cs](DotnetEnterpriseApi.Api/Controllers/AgentController.cs) | API | Calls `RetrieveContextAsync`, passes filter, returns `ragContext` in response |

---

## 🌐 Multi-Provider AI Support

The agent and RAG system are wired through the `Microsoft.Extensions.AI` abstraction layer (`IChatClient` / `IEmbeddingGenerator<string, Embedding<float>>`). The concrete provider is resolved at startup by `AiProviderFactory` based on a single config key — no code changes required to switch.

### Supported Providers

| Provider | `AI:Provider` value | SDK | Embeddings |
|---|---|---|---|
| OpenAI | `openai` | `Microsoft.Extensions.AI.OpenAI` | ✅ native |
| Anthropic Claude | `claude` | `Anthropic` 12.11.0 | ⚠️ falls back to OpenAI |
| Google Gemini | `gemini` | `Google.GenAI` 1.6.1 | ⚠️ falls back to OpenAI |
| Groq | `groq` | OpenAI-compatible endpoint | ⚠️ falls back to OpenAI |
| OpenRouter | `openrouter` | OpenAI-compatible endpoint | ✅ native (OpenAI-compat) |
| NVIDIA NIM | `nvidia` | OpenAI-compatible endpoint | ✅ native |
| xAI Grok | `grok` | OpenAI-compatible endpoint | ⚠️ falls back to OpenAI |

> ⚠️ "Falls back to OpenAI" means `AI:OpenAI:ApiKey` and `AI:OpenAI:EmbeddingModelId` must also be set when those providers are active, so that `IEmbeddingGenerator` can be satisfied.

### How to Switch

**1. Set the provider:**
```json
// appsettings.json
{
  "AI": {
    "Provider": "claude"
  }
}
```
Or override without editing the file:
```bash
dotnet user-secrets set "AI:Provider" "claude"
# or via environment variable
AI__Provider=claude
```

**2. Supply the provider's API key:**

```bash
# Claude
dotnet user-secrets set "AI:Claude:ApiKey" "sk-ant-..."
dotnet user-secrets set "AI:Claude:ModelId" "claude-sonnet-4-6"

# Gemini
dotnet user-secrets set "AI:Gemini:ApiKey" "..."
dotnet user-secrets set "AI:Gemini:ModelId" "gemini-2.0-flash"

# Groq
dotnet user-secrets set "AI:Groq:ApiKey" "gsk_..."
dotnet user-secrets set "AI:Groq:ModelId" "llama-3.3-70b-versatile"

# OpenRouter
dotnet user-secrets set "AI:Openrouter:ApiKey" "sk-or-..."
dotnet user-secrets set "AI:Openrouter:ModelId" "openai/gpt-4o-mini"

# NVIDIA NIM
dotnet user-secrets set "AI:Nvidia:ApiKey" "nvapi-..."
dotnet user-secrets set "AI:Nvidia:ModelId" "meta/llama-3.1-70b-instruct"
dotnet user-secrets set "AI:Nvidia:EmbeddingModelId" "nvidia/nv-embedqa-e5-v5"

# xAI Grok
dotnet user-secrets set "AI:Grok:ApiKey" "xai-..."
dotnet user-secrets set "AI:Grok:ModelId" "grok-3-mini"
```

**3. (If using a provider without native embeddings)** also set OpenAI as the embedding fallback:
```bash
dotnet user-secrets set "AI:OpenAI:ApiKey" "sk-..."
dotnet user-secrets set "AI:OpenAI:EmbeddingModelId" "text-embedding-3-small"
```

### Architecture

```
ServiceCollectionExtensions.AddAgentServices()
        │
        ▼
AiProviderFactory.CreateChatClient(configuration)
        │  reads AI:Provider, instantiates the matching SDK client
        ▼
IChatClient (registered as singleton)
        │
        ▼
AgentController — provider-agnostic from this point

AiProviderFactory.CreateEmbeddingGenerator(configuration)
        │  same switch; falls back to OpenAI for providers without native embeddings
        ▼
IEmbeddingGenerator<string, Embedding<float>> (registered as singleton)
        │
        ▼
TaskRagService — provider-agnostic from this point
```

### Key Files

| File | Purpose |
|---|---|
| [AiProviderFactory.cs](DotnetEnterpriseApi.Api/AI/AiProviderFactory.cs) | Provider switch — `CreateChatClient` + `CreateEmbeddingGenerator` |
| [ServiceCollectionExtensions.cs](DotnetEnterpriseApi.Api/Extensions/ServiceCollectionExtensions.cs) | `AddAgentServices()` calls the factory |
| [appsettings.json](DotnetEnterpriseApi.Api/appsettings.json) | Placeholder config blocks for all 7 providers |

---

## 🐘 pgvector Store

### Overview

When `DatabaseProvider = PostgreSQL`, the in-memory `ConcurrentDictionary` vector store is automatically replaced by a **PostgreSQL pgvector** implementation. Embeddings are persisted in a `task_embeddings` table — they survive application restarts without incurring rehydration API calls.

```
DatabaseProvider = PostgreSQL
        ↓
DependencyInjection selects PgVectorRagService (ITaskRagService)
        ↓
UpsertTaskEmbeddingAsync → INSERT / UPDATE task_embeddings (vector(1536) column)
        ↓
RetrieveContextAsync → ORDER BY embedding <=> $queryVector LIMIT topK
        ↓
Results ranked by cosine distance entirely inside Postgres
```

All other providers continue to use the in-memory store — no configuration change needed.

### Schema

```sql
CREATE TABLE task_embeddings (
    task_id      INTEGER PRIMARY KEY,
    title        TEXT    NOT NULL,
    description  TEXT    NOT NULL,
    is_completed BOOLEAN NOT NULL,
    embedding    vector(1536) NOT NULL,
    updated_at   TIMESTAMP NOT NULL
);
CREATE INDEX ON task_embeddings (is_completed);   -- metadata filter index
```

The table and index are created by EF Core migrations. Run `dotnet ef migrations add AddPgVector` and `dotnet ef database update` after switching to PostgreSQL.

### Switching to pgvector

```json
// appsettings.json
{
  "DataProvider":    "EntityFramework",
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=enterprisedb;Username=postgres;Password=..."
  }
}
```

The pgvector extension must be installed in your PostgreSQL instance:
```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### Key Files

| File | Purpose |
|---|---|
| [TaskEmbeddingRecord.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/TaskEmbeddingRecord.cs) | EF entity: maps to `task_embeddings` with `Vector` column type |
| [PgVectorRagService.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/PgVectorRagService.cs) | Full `ITaskRagService` backed by Postgres — upsert, cosine distance query, embedding cache |
| [AppDbContext.cs](DotnetEnterpriseApi.Infrastructure/Data/AppDbContext.cs) | `HasPostgresExtension("vector")` + `task_embeddings` table config (conditional on provider) |
| [DependencyInjection.cs](DotnetEnterpriseApi.Infrastructure/DependencyInjection.cs) | Auto-selects `PgVectorRagService` when PostgreSQL; registers `IDbContextFactory` for singleton use |

---

## 🔀 Hybrid Search

### Overview

Every RAG retrieval combines two independent ranking signals and merges them via **Reciprocal Rank Fusion (RRF)**:

| Signal | Method | Strength |
|---|---|---|
| **Vector** | Cosine similarity over embeddings | Semantic understanding — "deployment" matches "ship to prod" |
| **Keyword** | BM25 over title + description | Exact term matching — "JWT" retrieves JWT tasks specifically |
| **Fusion** | Reciprocal Rank Fusion (k=60) | Rewards items that rank well in *both* lists |

Hybrid search is **on by default** for both `/api/agent/chat` and `/api/multiagent/chat`. Pass `"hybrid": false` to force pure vector search.

### How RRF Works

```
Vector ranking:   [A(0.91), C(0.85), B(0.72), D(0.61)]
Keyword ranking:  [B(3.2),  A(2.8),  E(1.9),  C(1.1)]

RRF score = Σ 1 / (60 + rank_i)

  A: 1/61 + 1/62 = 0.0326   ← top — ranks well in both
  B: 1/63 + 1/60 = 0.0325   ← close second
  C: 1/62 + 1/63 = 0.0319
  E: 1/64 + 0    = 0.0156   ← keyword only
  D: 1/61 + 0    = 0.0164   ← vector only

Final: [A, B, C, D, E]
```

### Implementation

**In-memory store**: BM25 runs over all candidates in the `ConcurrentDictionary`.

**pgvector store**: Postgres fetches the top `5×topK` vector candidates via `ORDER BY embedding <=> query LIMIT N`; BM25 re-ranks them in C#. This keeps the SQL fast while providing keyword signal over the most semantically relevant candidates.

### BM25 Parameters

| Parameter | Value | Effect |
|---|---|---|
| `k1` | 1.5 | Term frequency saturation — diminishing returns after ~3 occurrences |
| `b` | 0.75 | Document length normalisation — penalises very long descriptions |
| IDF | Log-smoothed | Common terms across all tasks get lower weight |

### Request Options

```json
{
  "message": "find auth-related bugs",
  "hybrid": true,          // default — BM25 + vector via RRF
  "filterCompleted": false  // optional — pending tasks only
}
```

### Key Files

| File | Purpose |
|---|---|
| [HybridRanker.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/HybridRanker.cs) | `HybridRanker.Fuse()` — RRF fusion; `Bm25Scorer<T>` — pure C# BM25 implementation |
| [TaskRagService.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/TaskRagService.cs) | In-memory: BM25 over full store → RRF |
| [PgVectorRagService.cs](DotnetEnterpriseApi.Infrastructure/Repositories/VectorStore/PgVectorRagService.cs) | pgvector: top-N vector candidates → BM25 re-rank → RRF |

---

## 🤝 Multi-Agent Workflow

### Overview

`POST /api/multiagent/chat` routes every request through an **orchestrator** that classifies intent and delegates to the right specialised sub-agent. No routing logic is hard-coded — the Orchestrator uses the same LLM to reason about what the user wants.

```
User: "Create a bug task, then show me all pending work"
        ↓
OrchestratorAgent  →  intent = "both"
        ↓                ↓
MutationAgent        QueryAgent
(create the task)    (list pending + RAG context)
        ↓                ↓
        └──── merged reply ────┘
        ↓
{ reply, agentUsed: "MutationAgent + QueryAgent", intent: "both", ragContext: [...] }
```

### Intent Classification

A lightweight LLM call classifies each message into one of three buckets before any tools are invoked:

| Intent | Trigger examples | Sub-agent |
|---|---|---|
| `query` | "list tasks", "show pending", "how many done?", "what is task 5?" | QueryAgent |
| `mutation` | "create", "mark as done", "delete", "update the title" | MutationAgent |
| `both` | "create a task and show all pending", "complete task 3 then list open ones" | MutationAgent → QueryAgent (sequential) |

### QueryAgent

Read-only agent — cannot mutate state. RAG context is injected into its system instructions.

| Tool | Description |
|---|---|
| `ListTasksAsync` | Paginated task list with cursor support |
| `GetTaskByIdAsync` | Full detail for a single task |
| `CountTasksAsync` | Count tasks by status: `all` / `completed` / `pending` |

### MutationAgent

Write-only agent — no read tools. Confirms every action with the tool result.

| Tool | Description |
|---|---|
| `CreateTaskAsync` | Creates a new task |
| `UpdateTaskCompletionAsync` | Marks task completed / pending |
| `UpdateTaskDetailsAsync` | Updates title and/or description |
| `DeleteTaskAsync` | Permanently deletes a task |

### Usage Examples

```http
POST /api/multiagent/chat
Authorization: Bearer {token}
Content-Type: application/json

{ "message": "Show me all pending tasks" }
```
**Response:**
```json
{
  "reply": "[ID:1] Setup CI/CD — ⏳ Pending | Configure GitHub Actions\n[ID:4] Write docs — ⏳ Pending | ...",
  "agentUsed": "QueryAgent",
  "intent": "Query",
  "ragContext": [
    { "taskId": 1, "title": "Setup CI/CD", "isCompleted": false, "score": 0.8812 }
  ]
}
```

```http
POST /api/multiagent/chat
Authorization: Bearer {token}
Content-Type: application/json

{ "message": "Create a task 'Write release notes' then list all incomplete tasks" }
```
**Response:**
```json
{
  "reply": "✅ Task created. ID: 9, Title: \"Write release notes\"\n\n[ID:1] Setup CI/CD ...",
  "agentUsed": "MutationAgent + QueryAgent",
  "intent": "Both",
  "ragContext": [...]
}
```

### Architecture

```
POST /api/multiagent/chat
        │
        ▼
MultiAgentController
        │  1. RAG retrieval (hybrid search, topK=5)
        │  2. IntentClassifier LLM call → "query" | "mutation" | "both"
        ▼
┌───────────────────────────────┐
│  intent = query               │──→ QueryAgent (list/get/count tools + RAG context)
│  intent = mutation            │──→ MutationAgent (create/update/delete tools)
│  intent = both                │──→ MutationAgent → QueryAgent (sequential)
└───────────────────────────────┘
        │
        ▼
Merged reply + { agentUsed, intent, ragContext[] }
```

### Key Files

| File | Layer | Purpose |
|---|---|---|
| [MultiAgentController.cs](DotnetEnterpriseApi.Api/Controllers/MultiAgentController.cs) | API | Orchestrator: classify → route → merge |
| [QueryAgentTools.cs](DotnetEnterpriseApi.Application/Features/Tasks/AgentTools/QueryAgentTools.cs) | Application | 3 read tools: list, get, count |
| [MutationAgentTools.cs](DotnetEnterpriseApi.Application/Features/Tasks/AgentTools/MutationAgentTools.cs) | Application | 4 write tools: create, updateCompletion, updateDetails, delete |

---

## ⚙️ AI Workflow Automation Engine

### Overview

The Workflow Automation Engine lets you define and execute **multi-step LLM pipelines** via a simple REST API. Each workflow is a directed graph of steps; every step can call the LLM directly, invoke agent tools, or route execution based on a classifier verdict.

```
POST /api/workflow/run  { "workflowName": "triage", "input": "Login page throws 500 on prod" }
        │
        ▼
WorkflowEngine
    │
    ├─ Step 1 (Condition): classify → "critical"
    │       ↓ branch = "critical"
    ├─ Step 2 (Tool): create task [CRITICAL] …
    │       ↓ next
    └─ Step 3 (Llm): triage report summary
        │
        ▼
WorkflowExecution { executionId, status, steps[], finalOutput, durationMs }
```

### Step Types

| Type | Behaviour |
|---|---|
| `Llm` | Single `IChatClient.GetResponseAsync()` call; output appended to `{context}` |
| `Tool` | `IChatClient.AsAIAgent()` with the named tool set (query / mutation / all); agent calls tools autonomously |
| `Condition` | Single LLM call; trimmed reply is matched against `Branches` keys to select the next step |

### Prompt Template Variables

| Variable | Resolves to |
|---|---|
| `{input}` | The original user input passed to `POST /api/workflow/run` |
| `{context}` | All outputs accumulated so far (newline-separated `[Step: name]\n…` blocks) |

### Built-in Workflows

#### `triage`
Classifies a feature request or bug report, creates a prioritised task, and writes a triage report.

```
classify (Condition)
  ├─ "critical" → create-critical (Tool / mutation)
  ├─ "high"     → create-high    (Tool / mutation)
  ├─ "medium"   → create-medium  (Tool / mutation)
  └─ "low"      → create-low     (Tool / mutation)
            ↓
       summarise (Llm)
```

Task titles are auto-prefixed: `[CRITICAL]`, `[HIGH]`, `[LOW]` (medium has no prefix).

#### `daily-briefing`
Fetches all tasks and produces a prioritised daily plan with blocker analysis.

```
fetch (Tool / query)  →  prioritise (Llm)  →  identify-blockers (Llm)
```

Output includes: top-3 focus tasks, overdue/blocking items, and deferrable tasks.

#### `auto-close`
Identifies stale or already-done tasks and marks them completed automatically.

```
fetch (Tool / query)  →  analyse (Llm)  →  close (Tool / mutation)  →  report (Llm)
```

The analyser emits `CLOSE: <id>` lines; the close step calls `UpdateTaskCompletion` for each.

### Execution Model

- **Execution store** — `ConcurrentDictionary<string, WorkflowExecution>` (in-memory; replace with a DB-backed store for production).
- **Loop guard** — max 20 steps per execution to prevent infinite loops in mis-configured workflows.
- **Cancellation-aware** — every step checks `CancellationToken` before running.
- **Scoped tools** — each Tool step resolves `QueryAgentTools` / `MutationAgentTools` in a fresh `IServiceScope` so scoped dependencies are safe inside the singleton engine.

### API Endpoints

#### List registered workflows
```http
GET /api/workflow/definitions
Authorization: Bearer {token}
```
```json
[
  { "name": "triage",          "description": "Analyse a raw feature request ...", "entryStep": "classify",  "steps": [...] },
  { "name": "daily-briefing",  "description": "Fetch all tasks, produce a prioritised daily plan ...", "entryStep": "fetch", "steps": [...] },
  { "name": "auto-close",      "description": "Identify tasks that appear complete or stale ...", "entryStep": "fetch", "steps": [...] }
]
```

#### Run a workflow
```http
POST /api/workflow/run
Authorization: Bearer {token}
Content-Type: application/json

{
  "workflowName": "triage",
  "input": "The checkout button is broken on mobile — users can't complete purchases"
}
```
```json
{
  "executionId": "3f2e1a9b-...",
  "workflowName": "triage",
  "status": "Completed",
  "input": "The checkout button is broken ...",
  "finalOutput": "A CRITICAL task '[CRITICAL] Fix checkout button on mobile' was created ...",
  "durationMs": 4217,
  "steps": [
    { "stepName": "classify",        "stepType": "Condition", "output": "critical",  "branchTaken": "critical", "durationMs": 823 },
    { "stepName": "create-critical", "stepType": "Tool",      "output": "Task created: ID=14 ...",               "durationMs": 2891 },
    { "stepName": "summarise",       "stepType": "Llm",       "output": "A CRITICAL task was created ...",       "durationMs": 503 }
  ]
}
```

#### Poll execution status
```http
GET /api/workflow/{executionId}
Authorization: Bearer {token}
```
Returns the same `WorkflowExecutionResponse` structure above (useful for polling long-running workflows).

### Key Files

| File | Layer | Purpose |
|---|---|---|
| [WorkflowController.cs](DotnetEnterpriseApi.Api/Controllers/WorkflowController.cs) | API | REST endpoints: run, poll, list definitions |
| [IWorkflowEngine.cs](DotnetEnterpriseApi.Application/Interfaces/IWorkflowEngine.cs) | Application | Engine contract: `RunAsync`, `GetExecution`, `GetDefinitions` |
| [WorkflowModels.cs](DotnetEnterpriseApi.Application/Features/Workflow/Models/WorkflowModels.cs) | Application | Domain types: `WorkflowDefinition`, `WorkflowExecution`, `WorkflowStepType` |
| [BuiltInWorkflows.cs](DotnetEnterpriseApi.Application/Features/Workflow/Definitions/BuiltInWorkflows.cs) | Application | Static definitions: triage, daily-briefing, auto-close |
| [WorkflowEngine.cs](DotnetEnterpriseApi.Infrastructure/Workflow/WorkflowEngine.cs) | Infrastructure | Engine implementation: step loop, LLM/Tool/Condition runners |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Redis](https://redis.io/download/) (optional — for distributed caching)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/iamchittaranjandas/dotnet-enterprise-api.git
   cd dotnet-enterprise-api
   ```

2. **Update database connection string**
   
   Edit `DotnetEnterpriseApi.Api/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DotNetEnterpriseApiDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Update JWT settings**
   
   Edit `DotnetEnterpriseApi.Api/appsettings.json`:
   ```json
   "Jwt": {
     "Key": "YourSuperSecretKeyHere_MinimumLength32Characters!",
     "Issuer": "DotNetEnterpriseAPI",
     "Audience": "DotNetEnterpriseAPIUsers"
   }
   ```

4. **Configure AI provider for the Agent + RAG** (optional — only needed for `/api/agent/chat`)

   Set the provider and supply its API key via user secrets (recommended for development):
   ```bash
   cd DotnetEnterpriseApi.Api
   dotnet user-secrets set "AI:Provider" "openai"          # or claude, gemini, groq, openrouter, nvidia, grok
   dotnet user-secrets set "AI:OpenAI:ApiKey" "sk-..."
   dotnet user-secrets set "AI:OpenAI:ModelId" "gpt-4o-mini"
   dotnet user-secrets set "AI:OpenAI:EmbeddingModelId" "text-embedding-3-small"
   ```
   See [Multi-Provider AI Support](#-multi-provider-ai-support) for all provider config keys.

5. **Apply database migrations**
   ```bash
   cd DotnetEnterpriseApi.Api
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access Swagger UI**
   
   Navigate to: `https://localhost:5001/swagger`

---

## 📚 API Documentation

### Authentication Endpoints

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "User registered successfully",
  "data": {
    "id": 1,
    "userName": "john_doe",
    "email": "john@example.com"
  },
  "errors": []
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "userName": "john_doe",
    "email": "john@example.com",
    "role": "User"
  },
  "errors": []
}
```

### Task Management Endpoints

All task endpoints require authentication. Include the JWT token in the Authorization header:
```
Authorization: Bearer {your_jwt_token}
```

#### Get All Tasks
```http
GET /api/tasks
Authorization: Bearer {token}
```

#### Get Task by ID
```http
GET /api/tasks/{id}
Authorization: Bearer {token}
```

#### Create Task
```http
POST /api/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Complete project documentation",
  "description": "Write comprehensive README and API docs"
}
```

#### Update Task
```http
PUT /api/tasks/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "Updated title",
  "description": "Updated description",
  "isCompleted": true
}
```

#### Delete Task (Admin Only)
```http
DELETE /api/tasks/{id}
Authorization: Bearer {token}
```

### AI Agent Endpoints

#### Single Agent — RAG-Augmented
```http
POST /api/agent/chat
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "Create a task called 'Deploy to staging' with description 'Push latest build to staging environment'",
  "filterCompleted": null,
  "hybrid": true
}
```

**Response:**
```json
{
  "reply": "Task created successfully. ID: 8, Title: \"Deploy to staging\"",
  "ragContext": [
    { "taskId": 5, "title": "Deploy to prod", "isCompleted": false, "score": 0.8731 }
  ]
}
```

More examples:
```json
{ "message": "List all my tasks" }
{ "message": "Mark task 5 as completed" }
{ "message": "Show only pending tasks", "filterCompleted": false }
{ "message": "Delete task 2", "hybrid": false }
```

#### Multi-Agent — Orchestrated
```http
POST /api/multiagent/chat
Authorization: Bearer {token}
Content-Type: application/json

{ "message": "Create a task 'Write release notes', then list all pending tasks" }
```

**Response:**
```json
{
  "reply": "✅ Task created. ID: 9, Title: \"Write release notes\"\n\n[ID:1] Setup CI/CD — ⏳ Pending ...",
  "agentUsed": "MutationAgent + QueryAgent",
  "intent": "Both",
  "ragContext": [
    { "taskId": 1, "title": "Setup CI/CD", "isCompleted": false, "score": 0.8421 }
  ]
}
```

More examples:
```json
{ "message": "How many tasks are pending?" }
{ "message": "Mark task 3 as done" }
{ "message": "Show me all completed tasks", "filterCompleted": true }
```

#### Workflow Automation — Run
```http
POST /api/workflow/run
Authorization: Bearer {token}
Content-Type: application/json

{ "workflowName": "triage", "input": "Checkout button is broken on mobile" }
{ "workflowName": "daily-briefing", "input": "Generate today's plan" }
{ "workflowName": "auto-close", "input": "Clean up stale tasks" }
```

#### Workflow Automation — Poll
```http
GET /api/workflow/{executionId}
Authorization: Bearer {token}
```

#### Workflow Automation — List Definitions
```http
GET /api/workflow/definitions
Authorization: Bearer {token}
```

---

## 🔒 Security

- **JWT Authentication** - Secure token-based authentication
- **Password Hashing** - BCrypt with salt for password security
- **Role-Based Authorization** - Fine-grained access control
- **Input Validation** - FluentValidation prevents injection attacks
- **HTTPS** - Enforced encrypted communication

---

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## 📈 Performance

- **Pipeline Behaviors** - Automatic performance monitoring
- **Async/Await** - Non-blocking I/O operations
- **CQRS** - Optimized read and write operations
- **Logging** - Performance tracking for requests >500ms

---

## 🤝 Contributing

Contributions, suggestions, and bug reports are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for the full guide.

**Quick start:**
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes and test with all three data providers
4. Commit (`git commit -m 'Add: AmazingFeature'`)
5. Push and open a Pull Request

**Ways to contribute:**
- [Report a Bug](https://github.com/iamchittaranjandas/dotnet-enterprise-api/issues/new?template=bug_report.md)
- [Request a Feature](https://github.com/iamchittaranjandas/dotnet-enterprise-api/issues/new?template=feature_request.md)
- [Ask a Question](https://github.com/iamchittaranjandas/dotnet-enterprise-api/issues/new?template=question.md)
- Submit a Pull Request

Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Chittaranjan Das**
- GitHub: [@iamchittaranjandas](https://github.com/iamchittaranjandas)
- LinkedIn: [iamchittaranjandas](https://www.linkedin.com/in/iamchittaranjandas)

---

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- CQRS Pattern
- Domain-Driven Design by Eric Evans
- ASP.NET Core Team

---

**Built with ❤️ using Clean Architecture principles**
