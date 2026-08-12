using JobProcessingPlatform.Domain.Enums;
using JobProcessingPlatform.Domain.ValueObjects;

namespace JobProcessingPlatform.Domain.Entities;

public class Job
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public JobStatus Status { get; private set; }
    public JobPriority Priority { get; private set; }
    public string PayloadJson { get; private set; } = null!;
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? ResultJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public RetryPolicy RetryPolicy { get; private set; } = null!;
    public DateTime? ScheduledFor { get; private set; }
    public List<JobMetadata> Metadata { get; private set; } = new();

    public static Job Create(
        string name,
        string description,
        string payloadJson,
        Guid createdBy,
        JobPriority priority = JobPriority.Normal,
        RetryPolicy? retryPolicy = null,
        DateTime? scheduledFor = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Job name cannot be empty");
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Job payload cannot be empty");

        return new Job
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            PayloadJson = payloadJson,
            CreatedBy = createdBy,
            Status = JobStatus.Pending,
            Priority = priority,
            RetryPolicy = retryPolicy ?? RetryPolicy.Default,
            ScheduledFor = scheduledFor,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };
    }

    public void Start()
    {
        if (Status != JobStatus.Pending && Status != JobStatus.Retrying)
            throw new InvalidOperationException($"Cannot start job in {Status} status");

        Status = JobStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(string? resultJson = null)
    {
        if (Status != JobStatus.Running)
            throw new InvalidOperationException($"Cannot complete job in {Status} status");

        Status = JobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ResultJson = resultJson;
    }

    public void Fail(string errorMessage)
    {
        if (Status != JobStatus.Running)
            throw new InvalidOperationException($"Cannot fail job in {Status} status");

        ErrorMessage = errorMessage;

        if (RetryCount < RetryPolicy.MaxRetries)
        {
            Status = JobStatus.Retrying;
            RetryCount++;
        }
        else
        {
            Status = JobStatus.Failed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void Cancel()
    {
        if (Status == JobStatus.Completed || Status == JobStatus.Failed)
            throw new InvalidOperationException($"Cannot cancel job in {Status} status");

        Status = JobStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    public void AddMetadata(JobMetadata metadata)
    {
        if (Metadata.Any(m => m.Key == metadata.Key))
            throw new InvalidOperationException($"Metadata with key '{metadata.Key}' already exists");

        Metadata.Add(metadata);
    }

    public DateTime GetNextRetryTime()
    {
        if (Status != JobStatus.Retrying)
            throw new InvalidOperationException("Job is not in retrying status");

        var delaySeconds = Math.Min(
            RetryPolicy.InitialDelaySeconds * Math.Pow(RetryPolicy.BackoffMultiplier, RetryCount - 1),
            RetryPolicy.MaxDelaySeconds);

        return DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
