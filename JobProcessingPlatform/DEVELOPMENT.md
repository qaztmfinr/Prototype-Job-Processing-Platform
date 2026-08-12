# Development Guide

## Environment Setup

### Prerequisites
- Windows 10+ / macOS / Linux
- .NET 9 SDK ([download](https://dotnet.microsoft.com/en-us/download/dotnet/9.0))
- Docker & Docker Compose
- Visual Studio Code or Visual Studio
- Git

### Installation

1. **Clone Repository**
   ```bash
   git clone <your-repo-url>
   cd JobProcessingPlatform
   ```

2. **Install Dependencies**
   ```bash
   dotnet restore
   ```

3. **Start Docker Services**
   ```bash
   docker-compose up -d
   ```

4. **Run Database Migrations**
   ```bash
   cd src/JobProcessingPlatform.API
   dotnet ef database update
   ```

### Running Locally

#### Terminal 1: API
```bash
cd src/JobProcessingPlatform.API
dotnet watch run
```
- Swagger UI: `https://localhost:7000/swagger`
- API: `https://localhost:7000/api`

#### Terminal 2: Worker
```bash
cd src/JobProcessingPlatform.Worker
dotnet run
```

## Development Workflow

### Adding a New Feature

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/my-feature
   ```

2. **Update Domain Layer** (if new entity/logic)
   - Add entity or value object in `Domain/Entities` or `Domain/ValueObjects`
   - Add interface in `Domain/Interfaces` if needed

3. **Update Application Layer**
   - Create command/query in `Application/Commands` or `Application/Queries`
   - Create handler in `Application/Handlers`

4. **Update Infrastructure Layer**
   - Extend `DbContext` if new entities
   - Add repository implementation

5. **Add API Endpoint**
   - Add controller method in `API/Controllers`
   - Document with XML comments and Swagger attributes

6. **Add Tests**
   - Unit tests in `Tests/`
   - Use `[Fact]` for specific tests, `[Theory]` for parameterized

7. **Run Locally**
   ```bash
   dotnet run
   dotnet test
   ```

8. **Commit & Push**
   ```bash
   git add .
   git commit -m "feat: add my feature"
   git push origin feature/my-feature
   ```

9. **Create Pull Request** on GitHub

### Debugging

#### In Visual Studio Code
1. Install C# Extension
2. Place breakpoints
3. Press F5 to debug
4. Use Debug Console for evaluation

#### In Visual Studio
1. Open solution
2. Place breakpoints
3. Press F5 to debug
4. Use Watch/Immediate windows

#### With Docker
```bash
docker-compose logs -f api
docker-compose logs -f worker
```

## Database Operations

### View Database
```bash
# SQL Server
sqlcmd -S localhost,1433 -U sa -P YourPassword123!

# Query jobs
SELECT TOP 10 Id, Name, Status, CreatedAt FROM Jobs ORDER BY CreatedAt DESC;

# Query users
SELECT Id, Username, Email, Role, IsActive FROM Users;
```

### Reset Database
```bash
# With EF Core
dotnet ef database drop --force
dotnet ef database update

# With Docker
docker-compose down -v
docker-compose up -d
```

### Seed Data
Data is auto-seeded on first run:
- Username: `admin` / Password: `AdminPassword123!`
- Username: `user` / Password: `UserPassword123!`

## Redis Operations

### Check Queue
```bash
redis-cli
> LLEN job:queue:pending
> LRANGE job:queue:pending 0 -1
```

### Flush Redis
```bash
redis-cli
> FLUSHDB
> EXIT
```

## Code Style

### Naming Conventions
- **Classes:** PascalCase (e.g., `JobProcessingService`)
- **Methods:** PascalCase (e.g., `ProcessJobAsync`)
- **Properties:** PascalCase (e.g., `JobId`)
- **Parameters:** camelCase (e.g., `jobId`)
- **Interfaces:** PascalCase prefixed with I (e.g., `IJobRepository`)

### Documentation
```csharp
/// <summary>
/// Processes a job from the queue and handles retries.
/// </summary>
/// <param name="job">The job to process</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>True if job completed successfully</returns>
public async Task<bool> ProcessJobAsync(Job job, CancellationToken cancellationToken)
{
    // Implementation
}
```

### Project Structure
```
Feature/
├── Commands/
│   └── CreateXyzCommand.cs
├── Queries/
│   └── GetXyzQuery.cs
├── Handlers/
│   ├── CreateXyzCommandHandler.cs
│   └── GetXyzQueryHandler.cs
└── Services/
    └── IXyzService.cs
```

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test
```bash
dotnet test --filter "ClassName=JobProcessingPlatform.Tests.JobTests"
```

### With Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Adding New Tests
```csharp
[Fact]
public async Task CreateJob_WithValidData_ShouldSucceed()
{
    // Arrange
    var handler = new CreateJobCommandHandler(_jobRepoMock.Object, _queueMock.Object);
    var command = new CreateJobCommand(...);

    // Act
    var result = await handler.HandleAsync(command);

    // Assert
    Assert.NotEqual(Guid.Empty, result);
}
```

## Common Commands

```bash
# Build solution
dotnet build

# Clean build
dotnet clean && dotnet build

# Run tests
dotnet test

# Run API
dotnet run --project src/JobProcessingPlatform.API

# Run Worker
dotnet run --project src/JobProcessingPlatform.Worker

# Publish
dotnet publish src/JobProcessingPlatform.API -c Release -o ./publish

# Docker
docker-compose up -d
docker-compose down
docker-compose logs -f

# Git
git status
git pull origin develop
git push origin feature/my-feature
```

## Troubleshooting

### Port Already in Use
```bash
# Find process using port 7000
lsof -i :7000  # macOS/Linux
netstat -ano | findstr :7000  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows
```

### Database Connection Issues
- Check SQL Server is running: `docker-compose logs db`
- Verify connection string in `appsettings.Development.json`
- Ensure database permissions

### Redis Connection Issues
- Check Redis is running: `redis-cli ping`
- Verify connection string: `localhost:6379`
- Check firewall rules

### NuGet Package Issues
```bash
dotnet nuget locals all --clear
dotnet restore
```

## IDE Configuration

### Visual Studio Code
Install extensions:
- C# (ms-dotnettools.csharp)
- C# Extensions (jchannon.csharpextensions)
- REST Client (humao.rest-client)
- Docker (ms-azuretools.vscode-docker)

### Visual Studio
- File → New → Project → .NET 9 Console/Web App
- Solution Explorer for navigation
- Test Explorer for running tests (Test → Test Explorer)

## Performance Tips

1. **Async/Await** — Always use async operations
2. **Connection Pooling** — EF Core handles automatically
3. **Batch Operations** — Use bulk insert for seeding
4. **Indexes** — Added on Status, CreatedBy, CreatedAt
5. **Caching** — Consider for frequently accessed data
6. **Lazy Loading** — Disable if not needed (use eager load with Include())

## Security Checklist

- [ ] Change default JWT secret
- [ ] Use strong database password
- [ ] Enable HTTPS in production
- [ ] Validate all user inputs
- [ ] Review exception messages (don't leak internals)
- [ ] Test authorization on protected endpoints
- [ ] Use environment variables for secrets
- [ ] Enable rate limiting
- [ ] Set up audit logging

---

Happy coding! 🚀
