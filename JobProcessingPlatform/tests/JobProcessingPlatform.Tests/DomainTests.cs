using Xunit;
using Moq;
using JobProcessingPlatform.Domain.Entities;
using JobProcessingPlatform.Domain.Enums;
using JobProcessingPlatform.Application.Handlers;
using JobProcessingPlatform.Application.Commands;
using JobProcessingPlatform.Application.Exceptions;
using JobProcessingPlatform.Domain.Interfaces;

namespace JobProcessingPlatform.Tests;

public class JobTests
{
    [Fact]
    public void CreateJob_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "Test Job";
        var description = "A test job";
        var payload = "{\"data\": \"test\"}";

        // Act
        var job = Job.Create(name, description, payload, userId);

        // Assert
        Assert.NotEqual(Guid.Empty, job.Id);
        Assert.Equal(name, job.Name);
        Assert.Equal(description, job.Description);
        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Equal(userId, job.CreatedBy);
    }

    [Fact]
    public void CreateJob_WithEmptyName_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Job.Create("", "description", "{}", userId));
    }

    [Fact]
    public void StartJob_WithPendingStatus_ShouldChangeToRunning()
    {
        // Arrange
        var job = Job.Create("Test", "Test Job", "{}", Guid.NewGuid());

        // Act
        job.Start();

        // Assert
        Assert.Equal(JobStatus.Running, job.Status);
        Assert.NotNull(job.StartedAt);
    }

    [Fact]
    public void CompleteJob_WithRunningStatus_ShouldSucceed()
    {
        // Arrange
        var job = Job.Create("Test", "Test Job", "{}", Guid.NewGuid());
        job.Start();

        // Act
        job.Complete();

        // Assert
        Assert.Equal(JobStatus.Completed, job.Status);
        Assert.NotNull(job.CompletedAt);
    }

    [Fact]
    public void FailJob_WithoutReachingMaxRetries_ShouldMarkForRetry()
    {
        // Arrange
        var job = Job.Create("Test", "Test Job", "{}", Guid.NewGuid());
        job.Start();

        // Act
        job.Fail("Test error");

        // Assert
        Assert.Equal(JobStatus.Retrying, job.Status);
        Assert.Equal(1, job.RetryCount);
    }

    [Fact]
    public void FailJob_AfterMaxRetries_ShouldBeFailed()
    {
        // Arrange
        var retryPolicy = Domain.ValueObjects.RetryPolicy.Create(1, 60, 2.0, 3600);
        var job = Job.Create("Test", "Test Job", "{}", Guid.NewGuid(), retryPolicy: retryPolicy);
        job.Start();

        // Act
        job.Fail("Error 1");
        job.Start();
        job.Fail("Error 2");

        // Assert
        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal(2, job.RetryCount);
    }
}

public class CreateJobCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateJobAndEnqueue()
    {
        // Arrange
        var jobRepositoryMock = new Mock<IJobRepository>();
        var jobQueueMock = new Mock<IJobQueue>();
        var handler = new CreateJobCommandHandler(jobRepositoryMock.Object, jobQueueMock.Object);

        var command = new CreateJobCommand(
            "Test Job",
            "Test Description",
            "{}",
            Guid.NewGuid(),
            JobPriority.Normal,
            null,
            null);

        // Act
        var jobId = await handler.HandleAsync(command);

        // Assert
        Assert.NotEqual(Guid.Empty, jobId);
        jobRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Job>()), Times.Once);
        jobQueueMock.Verify(q => q.EnqueueAsync(It.IsAny<Job>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
    {
        // Arrange
        var jobRepositoryMock = new Mock<IJobRepository>();
        var jobQueueMock = new Mock<IJobQueue>();
        var handler = new CreateJobCommandHandler(jobRepositoryMock.Object, jobQueueMock.Object);

        var command = new CreateJobCommand(
            "",
            "Test Description",
            "{}",
            Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.HandleAsync(command));
    }
}

public class UserTests
{
    [Fact]
    public void CreateUser_WithValidParameters_ShouldSucceed()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var passwordHash = "hashedpassword";

        // Act
        var user = User.Create(username, email, passwordHash);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(username, user.Username);
        Assert.Equal(email, user.Email);
        Assert.Equal(UserRole.User, user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void DeactivateUser_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("testuser", "test@example.com", "hash");

        // Act
        user.Deactivate();

        // Assert
        Assert.False(user.IsActive);
    }
}
