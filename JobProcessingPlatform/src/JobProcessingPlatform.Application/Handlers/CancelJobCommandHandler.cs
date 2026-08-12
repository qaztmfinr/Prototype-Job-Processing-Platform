using JobProcessingPlatform.Application.Commands;
using JobProcessingPlatform.Domain.Interfaces;
using JobProcessingPlatform.Application.Exceptions;

namespace JobProcessingPlatform.Application.Handlers;

public class CancelJobCommandHandler
{
    private readonly IJobRepository _jobRepository;

    public CancelJobCommandHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task HandleAsync(CancelJobCommand command)
    {
        var job = await _jobRepository.GetByIdAsync(command.JobId);
        if (job == null)
            throw new NotFoundException($"Job with ID {command.JobId} not found");

        if (job.CreatedBy != command.RequestedBy && command.RequestedBy != Guid.Empty)
            throw new UnauthorizedException("You do not have permission to cancel this job");

        job.Cancel();
        await _jobRepository.UpdateAsync(job);
    }
}
