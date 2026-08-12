namespace JobProcessingPlatform.Domain.ValueObjects;

public record RetryPolicy
{
    public int MaxRetries { get; init; }
    public int InitialDelaySeconds { get; init; }
    public double BackoffMultiplier { get; init; }
    public int MaxDelaySeconds { get; init; }

    public static RetryPolicy Default => new()
    {
        MaxRetries = 3,
        InitialDelaySeconds = 60,
        BackoffMultiplier = 2.0,
        MaxDelaySeconds = 3600
    };

    public static RetryPolicy Create(int maxRetries, int initialDelaySeconds, double backoffMultiplier, int maxDelaySeconds)
    {
        if (maxRetries < 0)
            throw new ArgumentException("Max retries must be >= 0");
        if (initialDelaySeconds <= 0)
            throw new ArgumentException("Initial delay must be > 0");
        if (backoffMultiplier <= 1)
            throw new ArgumentException("Backoff multiplier must be > 1");
        if (maxDelaySeconds <= 0)
            throw new ArgumentException("Max delay must be > 0");

        return new RetryPolicy
        {
            MaxRetries = maxRetries,
            InitialDelaySeconds = initialDelaySeconds,
            BackoffMultiplier = backoffMultiplier,
            MaxDelaySeconds = maxDelaySeconds
        };
    }
}
