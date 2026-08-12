using JobProcessingPlatform.Application.Queries;
using JobProcessingPlatform.Domain.Interfaces;

namespace JobProcessingPlatform.Application.Handlers;

public class GetQueueStatsQueryHandler
{
    private readonly IJobQueue _jobQueue;
    private readonly IJobRepository _jobRepository;
    private readonly Domain.Enums.JobStatus[] _statuses;

    public GetQueueStatsQueryHandler(IJobQueue jobQueue, IJobRepository jobRepository)
    {
        _jobQueue = jobQueue;
        _jobRepository = jobRepository;
        _statuses = Enum.GetValues(typeof(Domain.Enums.JobStatus))
            .Cast<Domain.Enums.JobStatus>()
            .ToArray();
    }

    public async Task<Dictionary<string, int>> HandleAsync(GetQueueStatsQuery query)
    {
        var stats = new Dictionary<string, int>();

        foreach (var status in _statuses)
        {
            stats[status.ToString()] = await _jobRepository.GetCountByStatusAsync(status);
        }

        stats["QueueLength"] = await _jobQueue.GetCountAsync();

        return stats;
    }
}
