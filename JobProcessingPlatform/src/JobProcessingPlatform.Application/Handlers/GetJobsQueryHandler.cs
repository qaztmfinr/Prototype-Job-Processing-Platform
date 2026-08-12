using JobProcessingPlatform.Application.Queries;
using JobProcessingPlatform.Domain.Interfaces;
using JobProcessingPlatform.Domain.Enums;
using JobProcessingPlatform.Domain.Entities;

namespace JobProcessingPlatform.Application.Handlers;

public class GetJobsQueryHandler
{
    private readonly IJobRepository _jobRepository;

    public GetJobsQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IEnumerable<Job>> HandleAsync(GetJobsQuery query)
    {
        if (!string.IsNullOrEmpty(query.Status) && Enum.TryParse<JobStatus>(query.Status, out var status))
        {
            return await _jobRepository.GetByStatusAsync(status, query.Skip, query.Take);
        }

        // Default: return paginated jobs by status (Pending first)
        return await _jobRepository.GetByStatusAsync(JobStatus.Pending, query.Skip, query.Take);
    }
}
