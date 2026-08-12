# Architecture Documentation

## Overview

The Job Processing Platform is built using **Clean Architecture** principles, separating concerns into distinct layers with clear dependencies:

```
┌─────────────────────────────────────────────────────────┐
│              ASP.NET Core REST API / Worker             │
├─────────────────────────────────────────────────────────┤
│  Controllers | Middleware | Program Configuration      │
├─────────────────────────────────────────────────────────┤
│              Application Layer                          │
│  Commands | Queries | Handlers | Exceptions | DTOs     │
├─────────────────────────────────────────────────────────┤
│              Domain Layer                               │
│  Entities | Value Objects | Enums | Interfaces         │
├─────────────────────────────────────────────────────────┤
│         Infrastructure Layer                            │
│  EF Core | Redis | Repositories | Authentication       │
└─────────────────────────────────────────────────────────┘
```

## Layers

### 1. **Domain Layer** (`JobProcessingPlatform.Domain`)

The **innermost layer** containing pure business logic with no external dependencies.

**Entities:**
- `Job` — Represents a background job with status, retries, and metadata
- `User` — Represents platform users with roles and authentication

**Value Objects:**
- `JobMetadata` — Key-value pairs attached to jobs
- `RetryPolicy` — Encapsulates retry strategy with backoff calculation

**Enums:**
- `JobStatus` — Pending, Running, Completed, Failed, Retrying, Cancelled
- `JobPriority` — Low, Normal, High, Critical
- `UserRole` — Admin, Manager, User

**Interfaces (Ports):**
- `IJobRepository` — Job persistence contract
- `IUserRepository` — User persistence contract
- `IJobQueue` — Job queue contract (implemented by Redis)
- `IJobProcessor` — Job processing contract

### 2. **Application Layer** (`JobProcessingPlatform.Application`)

**Orchestrates business use cases** using CQRS pattern.

**Commands** (state-changing operations):
- `CreateJobCommand` — Create a new job
- `CancelJobCommand` — Cancel a job
- `RegisterUserCommand` — Register a new user

**Queries** (read operations):
- `GetJobQuery` — Retrieve single job
- `GetJobsQuery` — Retrieve paginated jobs
- `GetQueueStatsQuery` — Get queue statistics
- `LoginQuery` — User authentication

**Handlers** (execute commands/queries):
- `CreateJobCommandHandler` — Creates job + enqueues
- `CancelJobCommandHandler` — Cancels job with authorization
- `GetJobQueryHandler` — Retrieves job with not-found handling
- `GetJobsQueryHandler` — Retrieves paginated results
- `GetQueueStatsQueryHandler` — Calculates queue statistics

**Exceptions** (custom domain exceptions):
- `NotFoundException` — 404 errors
- `ValidationException` — 400 validation errors
- `UnauthorizedException` — 401 authorization failures
- `JobProcessingException` — Domain-specific job errors

**Services** (abstracted interfaces):
- `ITokenService` — JWT token generation/validation
- `IPasswordService` — Password hashing/verification

### 3. **Infrastructure Layer** (`JobProcessingPlatform.Infrastructure`)

**Implements external adapters** (databases, queues, authentication).

**Persistence:**
- `JobProcessingDbContext` — EF Core DbContext with Job and User entities
- Supports SQL Server & PostgreSQL

**Repositories:**
- `JobRepository` — Implements `IJobRepository` using EF Core
- `UserRepository` — Implements `IUserRepository` using EF Core

**Queue:**
- `RedisJobQueue` — Implements `IJobQueue` using Redis
  - Enqueue by priority
  - Dequeue FIFO
  - Requeue for retries
  - Track queue length

**Authentication:**
- `TokenService` — JWT token generation with configurable expiration
- `PasswordService` — SHA256 password hashing and verification

### 4. **API Layer** (`JobProcessingPlatform.API`)

**REST endpoints** with exception handling and authentication.

**Controllers:**
- `JobsController` — POST/GET/DELETE jobs
- `AuthController` — Register/Login
- `StatsController` — Queue statistics and health checks

**Middleware:**
- `ExceptionHandlingMiddleware` — Centralized error handling
  - Catches domain exceptions
  - Maps to HTTP status codes
  - Returns structured error responses

**Configuration:**
- `Program.cs` — Service registration, authentication setup, database initialization
- `appsettings.json` — Environment-specific configuration

### 5. **Worker Service** (`JobProcessingPlatform.Worker`)

**Scalable background job processor** polling Redis queue.

**Features:**
- Continuously polls Redis queue
- Processes jobs with error handling
- Automatic retry management
- Graceful shutdown handling
- Configurable polling interval

**Job Processing Flow:**
1. Dequeue from Redis
2. Change status to Running
3. Execute job logic (simulated with delay + 10% failure rate)
4. On success: Mark Completed
5. On failure: Check retry policy
   - If retries available: Mark Retrying, increment counter
   - If max retries reached: Mark Failed

## Data Flow

### Creating a Job

```
POST /api/jobs
    ↓
JobsController.CreateJob()
    ↓
CreateJobCommandHandler.HandleAsync()
    ↓
Job.Create() (Domain)
    ↓
JobRepository.AddAsync() (EF Core → Database)
JobQueue.EnqueueAsync() (Redis)
    ↓
201 Created { jobId }
```

### Processing a Job

```
JobWorkerService polling every 5 seconds
    ↓
JobQueue.DequeueAsync() (Redis LPOP)
    ↓
Job.Start() → Status = Running
    ↓
ProcessJobAsync() (Simulated)
    ↓
On Success: Job.Complete() → Completed
On Failure: Job.Fail() → Retrying or Failed
    ↓
JobRepository.UpdateAsync() (Persist status change)
```

## Authentication & Authorization

### Flow
1. User calls `POST /api/auth/login`
2. Credentials verified against hashed password in DB
3. `TokenService.GenerateToken()` creates JWT with:
   - `sub` (user ID)
   - `name` (username)
   - `role` (Admin/Manager/User)
   - `exp` (1 hour expiration)
4. Client includes token: `Authorization: Bearer <token>`
5. `JwtBearerDefaults` validates on protected endpoints

### Protected Endpoints
- `POST /api/jobs` — Requires authenticated user
- `DELETE /api/jobs/{id}` — Requires job ownership or Admin
- `GET /api/stats` — Requires authenticated user

## Testing Strategy

### Unit Tests (Domain & Application)
- Test entity state transitions
- Test command/query handlers
- Test business rule validation
- Use Moq for repository mocks

### Integration Tests (Future)
- In-memory database context
- Real repository implementations
- End-to-end workflows

### Test Files
- `DomainTests.cs` — Job and User entity tests
- `ValueObjectTests.cs` — RetryPolicy and JobMetadata tests

## Deployment Architecture

```
Load Balancer
    ↓
┌───────────────────────────┐
│   API Instance 1          │
│   - REST Endpoints        │
│   - JWT Auth              │
└───────────────────────────┘
    ↓ (Shared Database)
┌───────────────────────────┐
│   SQL Server / PostgreSQL │
│   - Jobs, Users           │
└───────────────────────────┘
    ↓ (Shared Queue)
┌───────────────────────────┐
│   Redis Cluster           │
│   - Job Queue             │
└───────────────────────────┘
    ↑
┌───────────────────────────┐
│   Worker Instance 1       │
│   - Poll Redis            │
│   - Process Jobs          │
└───────────────────────────┘

┌───────────────────────────┐
│   Worker Instance N       │
│   - Poll Redis            │
│   - Process Jobs          │
└───────────────────────────┘
```

## Scalability Considerations

1. **Horizontal API Scaling** — Stateless REST endpoints
2. **Horizontal Worker Scaling** — Multiple workers competing for queue
3. **Database Optimization** — Indexes on Status, CreatedBy, CreatedAt
4. **Redis Persistence** — RDB snapshots or AOF
5. **Rate Limiting** — Can be added to controllers
6. **Caching** — Opportunities at API layer
7. **Async Processing** — Job payload offloaded from request path

## Security Measures

1. **JWT Tokens** — HS256 signing with strong secret (32+ chars)
2. **Password Hashing** — SHA256 with salting (can upgrade to bcrypt)
3. **Role-Based Access** — User/Manager/Admin roles
4. **Input Validation** — Domain validation in entities
5. **Exception Handling** — No sensitive details in error responses
6. **CORS** — Configured per environment
7. **HTTPS** — Enforced in production

## Configuration Management

**appsettings.json** structure:
```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "Database": { "Provider": "SqlServer|PostgreSQL" },
  "Redis": { "Connection": "..." },
  "Jwt": { "Secret": "...", "Issuer": "...", "Audience": "..." },
  "Logging": { "LogLevel": { "Default": "..." } }
}
```

Environment-specific overrides:
- `appsettings.Development.json`
- `appsettings.Production.json`

## Extension Points

1. **Job Processors** — Implement `IJobProcessor` for custom job types
2. **Queue Implementations** — Swap Redis for RabbitMQ, Azure Queue, etc.
3. **Database Providers** — Already supports SQL Server and PostgreSQL
4. **Authentication** — Replace JWT with OAuth2, Azure AD, etc.
5. **Monitoring** — Add Prometheus, Application Insights
6. **Messaging** — Integrate event bus for domain events

---

This architecture emphasizes **separation of concerns**, **testability**, **scalability**, and **production-readiness** — key aspects appreciated in technical interviews for backend engineer roles.
