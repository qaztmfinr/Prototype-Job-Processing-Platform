using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using JobProcessingPlatform.Infrastructure.Persistence;
using JobProcessingPlatform.Infrastructure.Repositories;
using JobProcessingPlatform.Infrastructure.Queue;
using JobProcessingPlatform.Infrastructure.Authentication;
using JobProcessingPlatform.Domain.Interfaces;
using JobProcessingPlatform.Application.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    var jwtSecret = context.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
    var dbProvider = context.Configuration["Database:Provider"] ?? "PostgreSQL";
    var connectionString = context.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not configured");
    var redisConnection = context.Configuration["Redis:Connection"] ?? "localhost:6379";

    // Database
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        services.AddDbContext<JobProcessingDbContext>(options =>
            options.UseSqlServer(connectionString));
    }
    else
    {
        services.AddDbContext<JobProcessingDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    // Redis
    var redis = ConnectionMultiplexer.Connect(redisConnection);
    services.AddSingleton(redis);

    // Repositories & Queue
    services.AddScoped<IJobRepository, JobRepository>();
    services.AddScoped<IJobQueue, RedisJobQueue>();

    // Authentication
    services.AddScoped<IPasswordService, PasswordService>();
    services.AddScoped<ITokenService>(sp => new TokenService(jwtSecret, "JobProcessingPlatform", "JobProcessingPlatformAPI", 60));

    // Worker Service
    services.AddHostedService<JobWorkerService>();
});

builder.ConfigureLogging((context, logging) =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var host = builder.Build();
await host.RunAsync();

public class JobWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobWorkerService> _logger;
    private const int PollingIntervalSeconds = 5;

    public JobWorkerService(IServiceProvider serviceProvider, ILogger<JobWorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Worker Service started at {time}", DateTime.UtcNow);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
                    var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

                    // Dequeue pending jobs
                    var job = await jobQueue.DequeueAsync();
                    if (job == null)
                    {
                        // Check for retrying jobs
                        var retryingJobs = await jobRepository.GetRetryingJobsAsync(10);
                        foreach (var retryingJob in retryingJobs)
                        {
                            var nextRetryTime = retryingJob.GetNextRetryTime();
                            if (DateTime.UtcNow >= nextRetryTime)
                            {
                                _logger.LogInformation("Retrying job {JobId}", retryingJob.Id);
                                await jobQueue.RequeueAsync(retryingJob);
                            }
                        }

                        await Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), stoppingToken);
                        continue;
                    }

                    try
                    {
                        _logger.LogInformation("Processing job {JobId}: {JobName}", job.Id, job.Name);

                        job.Start();
                        await jobRepository.UpdateAsync(job);

                        // Simulate processing
                        await ProcessJobAsync(job, stoppingToken);

                        job.Complete(System.Text.Json.JsonSerializer.Serialize(new { status = "completed", processedAt = DateTime.UtcNow }));
                        await jobRepository.UpdateAsync(job);

                        _logger.LogInformation("Job {JobId} completed successfully", job.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing job {JobId}", job.Id);
                        job.Fail(ex.Message);
                        await jobRepository.UpdateAsync(job);

                        if (job.Status == Domain.Enums.JobStatus.Retrying)
                        {
                            _logger.LogInformation("Job {JobId} marked for retry (attempt {RetryCount})", job.Id, job.RetryCount);
                            await jobQueue.RequeueAsync(job);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in worker service");
            }

            await Task.Delay(TimeSpan.FromSeconds(PollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Job Worker Service stopped at {time}", DateTime.UtcNow);
    }

    private async Task ProcessJobAsync(Domain.Entities.Job job, CancellationToken cancellationToken)
    {
        // Simulate job processing with configurable delay
        var delay = Random.Shared.Next(100, 2000);
        await Task.Delay(delay, cancellationToken);

        // Simulate occasional failures (10% chance)
        if (Random.Shared.Next(0, 100) < 10)
        {
            throw new InvalidOperationException("Simulated job processing failure");
        }
    }
}
