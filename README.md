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
- **Entity Framework Core** - Code-first database approach
- **Multi-Provider Data Access** - Choose between Entity Framework, Dapper, or ADO.NET via config

---

## 📦 Technologies & Packages

- **ASP.NET Core Web API** (.NET 10)
- **MediatR** (12.4.1) - CQRS implementation
- **FluentValidation** (11.11.0) - Request validation
- **Entity Framework Core** (10.0.5) - Full ORM with change tracking & migrations
- **Dapper** (2.1.72) - Lightweight micro-ORM with raw SQL
- **ADO.NET** (Microsoft.Data.SqlClient) - Pure data access, no ORM
- **SQL Server**
- **JWT Bearer Authentication**
- **BCrypt.Net** - Secure password hashing
- **Mapster** (7.4.0) - Object mapping
- **Swagger/OpenAPI** - API documentation

---

## 🏛️ Clean Architecture Structure

### 1. **API Layer** (`DotnetEnterpriseApi.Api`)
**Responsibilities:**
- HTTP request/response handling
- Controllers (thin, delegating to MediatR)
- Middleware (Exception handling, Request logging)
- Dependency injection configuration
- Authentication & Authorization setup
- Swagger configuration

**Key Files:**
- `Controllers/` - API endpoints using MediatR
- `Middleware/` - Custom middleware components
- `Program.cs` - Application startup and DI configuration

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
│   └── Models/              # Result pattern, common models
├── Features/
│   ├── Authentication/
│   │   ├── Commands/        # Register, Login commands
│   │   └── EventHandlers/   # Domain event handlers
│   └── Tasks/
│       ├── Commands/        # Create, Update, Delete commands
│       ├── Queries/         # GetAll, GetById queries
│       └── EventHandlers/   # Task-related event handlers
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
- Data provider selection via configuration
- Data persistence

**Structure:**
```
Infrastructure/
├── Data/
│   ├── AppDbContext.cs            # EF Core DbContext with domain events
│   └── SqlConnectionFactory.cs    # IDbConnection factory (Dapper & ADO.NET)
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
| `Ado` | Pure ADO.NET with SqlCommand/SqlDataReader, no ORM | Maximum control, zero abstraction overhead |

### How It Works

```
appsettings.json ["DataProvider"]
        │
        ▼
DependencyInjection.cs (Infrastructure)
        │
        ├── "EntityFramework"
        │       ├── AppDbContext (EF Core)
        │       ├── UnitOfWork (EF transactions)
        │       ├── EfTaskRepository
        │       └── EfUserRepository
        │
        ├── "Dapper"
        │       ├── SqlConnectionFactory (IDbConnection)
        │       ├── DapperUnitOfWork
        │       ├── DapperTaskRepository
        │       └── DapperUserRepository
        │
        └── "Ado"
                ├── SqlConnectionFactory (IDbConnection)
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
SQL Server Database
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
- Response: `Result<LoginResponse>` with JWT token

### Task Management Endpoints (Requires Authentication)

**GET** `/api/tasks`
- Get all tasks
- Response: `Result<List<TaskResponse>>`

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

---

---

## � Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

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

4. **Apply database migrations**
   ```bash
   cd DotnetEnterpriseApi.Api
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access Swagger UI**
   
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

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

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
