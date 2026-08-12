using Xunit;
using JobProcessingPlatform.Domain.ValueObjects;

namespace JobProcessingPlatform.Tests;

public class ValueObjectTests
{
    [Fact]
    public void RetryPolicy_Create_WithValidParameters_ShouldSucceed()
    {
        // Act
        var policy = RetryPolicy.Create(3, 60, 2.0, 3600);

        // Assert
        Assert.Equal(3, policy.MaxRetries);
        Assert.Equal(60, policy.InitialDelaySeconds);
        Assert.Equal(2.0, policy.BackoffMultiplier);
        Assert.Equal(3600, policy.MaxDelaySeconds);
    }

    [Fact]
    public void RetryPolicy_Create_WithInvalidMaxRetries_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RetryPolicy.Create(-1, 60, 2.0, 3600));
    }

    [Fact]
    public void JobMetadata_Create_WithValidParameters_ShouldSucceed()
    {
        // Act
        var metadata = JobMetadata.Create("key1", "value1");

        // Assert
        Assert.Equal("key1", metadata.Key);
        Assert.Equal("value1", metadata.Value);
    }

    [Fact]
    public void JobMetadata_Create_WithEmptyKey_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            JobMetadata.Create("", "value"));
    }
}
