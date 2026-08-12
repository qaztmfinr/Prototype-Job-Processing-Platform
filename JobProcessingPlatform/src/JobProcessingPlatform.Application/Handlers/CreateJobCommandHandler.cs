using JobProcessingPlatform.Application.Commands;
using JobProcessingPlatform.Domain.Entities;
using JobProcessingPlatform.Domain.Interfaces;
using JobProcessingPlatform.Domain.ValueObjects;
using JobProcessingPlatform.Application.Exceptions;

namespace JobProcessingPlatform.Application.Handlers;

public class CreateJobCommandHandler
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueue _jobQueue;

    public CreateJobCommandHandler(IJobRepository jobRepository, IJobQueue jobQueue)
    {
        _jobRepository = jobRepository;
        _jobQueue = jobQueue;
    }

    public async Task<Guid> HandleAsync(CreateJobCommand command)
    {
        try
        {
            var retryPolicy = new Domain.ValueObjects.RetryPolicy
            {
                MaxRetries = command.MaxRetries ?? 3,
                InitialDelaySeconds = command.InitialDelaySeconds ?? 60,
                BackoffMultiplier = 2.0,
                MaxDelaySeconds = 3600
            };

            var job = Job.Create(
                command.Name,
                command.Description,
                command.PayloadJson,
                command.CreatedBy,
                command.Priority,
                retryPolicy);

            await _jobRepository.AddAsync(job);
            await _jobQueue.EnqueueAsync(job);

            return job.Id;
        }
        catch (ArgumentException ex)
        {
            throw new ValidationException($"Invalid job creation parameters: {ex.Message}");
        }
    }
}
