# Job Processing Platform

A distributed job processing platform built with ASP.NET Core, Entity Framework Core, Redis, and Docker. The system implements async processing patterns, message queues, retry logic, and DevOps infrastructure.

## Features

- Distributed job queue backed by Redis with priority-based processing
- Retry logic with exponential backoff
- JWT authentication and role-based access control (Admin, Manager, User)
- Entity Framework Core with SQL Server and PostgreSQL support
- Background worker service for job processing
- Swagger/OpenAPI API documentation
- Centralized exception handling middleware
- Unit tests with XUnit and Moq
- Docker and Docker Compose configuration
- GitHub Actions CI/CD pipelines
- Structured logging to console and debug output
- Layered architecture (Domain, Application, Infrastructure, API)

## Architecture

```
JobProcessingPlatform/
├── src/
│   ├── JobProcessingPlatform.Domain/          # Domain entities, enums, interfaces
│   ├── JobProcessingPlatform.Application/     # Commands, queries, handlers, DTOs
│   ├── JobProcessingPlatform.Infrastructure/  # EF Core, Redis, Auth, Repositories
│   ├── JobProcessingPlatform.API/             # ASP.NET Core REST API
│   └── JobProcessingPlatform.Worker/          # Background worker service
├── tests/
│   └── JobProcessingPlatform.Tests/           # Unit & integration tests
├── .github/workflows/                          # CI/CD pipelines
├── docker-compose.yml                          # Local development
├── Dockerfile.API                              # API container
├── Dockerfile.Worker                           # Worker container
└── README.md
```

## Quick Start

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose
- Redis (or use Docker)

### Local Development

1. **Clone & Setup**
   ```bash
   git clone <your-repo-url>
   cd JobProcessingPlatform
   ```

2. **Start Services with Docker Compose**
   ```bash
   docker-compose up -d
   ```
   Services start:
   - SQL Server: `localhost:1433` (sa/YourPassword123!)
   - Redis: `localhost:6379`

3. **Run API**
   ```bash
   cd src/JobProcessingPlatform.API
   dotnet run
   ```
   - API: `https://localhost:7000`
   - Swagger: `https://localhost:7000/swagger`

4. **Run Worker** (in separate terminal)
   ```bash
   cd src/JobProcessingPlatform.Worker
   dotnet run
   ```

### Using Docker Compose

Start all services with:
```bash
docker-compose up -d
```

This starts:
- API on `http://localhost:5000`
- Worker service continuously processing jobs from Redis
- SQL Server on `localhost:1433`
- Redis on `localhost:6379`

## API Endpoints

### Authentication
```bash
# Register
POST /api/auth/register
Content-Type: application/json
{
  "username": "user1",
  "email": "user@example.com",
  "password": "Password123!"
}

# Login
POST /api/auth/login
{
  "username": "user1",
  "password": "Password123!"
}
```

### Jobs
```bash
# Get JWT Token First
TOKEN=$(curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"AdminPassword123!"}' | jq -r '.token')

# Create Job
POST /api/jobs
Authorization: Bearer $TOKEN
Content-Type: application/json
{
  "name": "ProcessData",
  "description": "Process user data",
  "payloadJson": "{\"userId\": 123}",
  "priority": 1
}

# Get Job
GET /api/jobs/{jobId}
Authorization: Bearer $TOKEN

# List Jobs (with pagination)
GET /api/jobs?skip=0&take=10&status=Completed
Authorization: Bearer $TOKEN

# Cancel Job
DELETE /api/jobs/{jobId}
Authorization: Bearer $TOKEN
```

### Monitoring
```bash
# Queue Statistics
GET /api/stats
Authorization: Bearer $TOKEN

# Health Check
GET /api/stats/health
```

## Default Credentials

| User | Password | Role |
|------|----------|------|
| admin | AdminPassword123! | Admin |
| user | UserPassword123! | User |

## Database

### SQL Server
```bash
Server=localhost;Database=JobProcessingPlatform;User Id=sa;Password=YourPassword123!;
```

### PostgreSQL
Uncomment the postgres service in docker-compose.yml and use:
```bash
Server=postgres;Database=JobProcessingPlatform;User Id=postgres;Password=postgres;
```

## Docker

### Build Images
```bash
# Build API
docker build -f Dockerfile.API -t jobprocessing-api:latest .

# Build Worker
docker build -f Dockerfile.Worker -t jobprocessing-worker:latest .
```

### Run with Docker Compose
```bash
docker-compose up -d
docker-compose logs -f
docker-compose down
```

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Run specific test class
dotnet test --filter "ClassName=JobProcessingPlatform.Tests.JobTests"
```

## CI/CD Pipelines

### Build & Test

Located at `.github/workflows/build-test.yml`
- Runs on push to `main` and `develop` branches
- Executes dotnet build
- Runs unit tests
- Publishes build artifacts

### Docker Build & Push

Located at `.github/workflows/docker-build.yml`
- Builds Docker images when tags are pushed or on main branch
- Pushes images to GitHub Container Registry
- Supports semantic versioning for images

## Job Processing Flow

1. Create Job — POST `/api/jobs` — Stored in DB
2. Enqueue — Job added to Redis queue (priority-sorted)
3. Worker Poll — Background service checks Redis
4. Process — Job status changes to Running
5. Complete/Fail — Update job status, handle retries
6. Retry Logic — Exponential backoff if configured

## Scalability

- Horizontal scaling by running multiple worker instances
- Priority queue for job ordering
- Automatic retries with backoff
- Health check endpoints at `/api/stats/health`
- Structured logging for monitoring
- Database indexes on Status, CreatedBy, and CreatedAt columns

## Technologies

| Layer | Technology |
|-------|-----------|
| API | ASP.NET Core 9 |
| ORM | Entity Framework Core 9 |
| Queue | Redis 7 |
| Database | SQL Server 2022 / PostgreSQL 16 |
| Auth | JWT Bearer + Role-based |
| Testing | XUnit + Moq |
| Container | Docker |
| CI/CD | GitHub Actions |
| Documentation | Swagger/OpenAPI 3.0 |

## Configuration

### Environment Variables

**API/Worker:**
```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=JobProcessingPlatform;...
Database__Provider=SqlServer  # or PostgreSQL
Redis__Connection=localhost:6379
Jwt__Secret=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256
Jwt__Issuer=JobProcessingPlatform
Jwt__Audience=JobProcessingPlatformAPI
ASPNETCORE_ENVIRONMENT=Development  # Development|Production
```

See `appsettings.json` and `appsettings.Production.json` for full configuration options.

## Troubleshooting

### Redis Connection Failed
```bash
# Check Redis is running
docker-compose logs redis

# Test Redis connection
redis-cli -h localhost ping
```

### Database Connection Failed
```bash
# Check SQL Server is running
docker-compose logs db

# Verify connection string in appsettings.json
```

### Worker Not Processing Jobs
```bash
# Check worker logs
docker-compose logs worker

# Verify Redis queue has jobs
redis-cli LLEN job:queue:pending
```

## Production Deployment

### Environment Preparation

1. Update `appsettings.Production.json` with production database and Redis connection strings
2. Generate a strong JWT secret (minimum 32 characters)
3. Configure health check endpoints
4. Set up logging and monitoring infrastructure

### Kubernetes Deployment

Build and push images:
```bash
docker build -f Dockerfile.API -t your-registry/api:v1.0 .
docker push your-registry/api:v1.0

docker build -f Dockerfile.Worker -t your-registry/worker:v1.0 .
docker push your-registry/worker:v1.0
```

Deploy using kubectl:
```bash
kubectl apply -f k8s/
```

## Learning Resources

- [Clean Architecture in .NET](https://github.com/jasontaylordev/CleanArchitecture)
- [Vertical Slice Architecture](https://codeopinion.com/vertical-slice-architecture/)
- [Redis & Job Queues](https://redis.io/)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core)

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/description-of-feature`
3. Commit changes: `git commit -m 'Add feature'`
4. Push to branch: `git push origin feature/description-of-feature`
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or feedback, open a GitHub Issue or Discussion.

---

This project is designed for backend .NET engineers to understand and demonstrate distributed job processing systems, async patterns, and infrastructure architecture.
