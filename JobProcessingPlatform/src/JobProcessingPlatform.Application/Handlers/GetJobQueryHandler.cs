using JobProcessingPlatform.Application.Queries;
using JobProcessingPlatform.Domain.Interfaces;
using JobProcessingPlatform.Application.Exceptions;
using JobProcessingPlatform.Domain.Entities;

namespace JobProcessingPlatform.Application.Handlers;

public class GetJobQueryHandler
{
    private readonly IJobRepository _jobRepository;

    public GetJobQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<Job?> HandleAsync(GetJobQuery query)
    {
        var job = await _jobRepository.GetByIdAsync(query.JobId);
        if (job == null)
            throw new NotFoundException($"Job with ID {query.JobId} not found");

        return job;
    }
}
