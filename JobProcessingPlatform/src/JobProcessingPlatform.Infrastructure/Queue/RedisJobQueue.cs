using System.Text.Json;
using JobProcessingPlatform.Domain.Entities;
using JobProcessingPlatform.Domain.Interfaces;
using StackExchange.Redis;

namespace JobProcessingPlatform.Infrastructure.Queue;

public class RedisJobQueue : IJobQueue
{
    private readonly IDatabase _db;
    private const string JobQueueKey = "job:queue:pending";
    private const string JobKey = "job:";

    public RedisJobQueue(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task EnqueueAsync(Job job)
    {
        var json = JsonSerializer.Serialize(job);
        await _db.StringSetAsync($"{JobKey}{job.Id}", json);
        await _db.ListRightPushAsync(JobQueueKey, job.Id.ToString());
    }

    public async Task<Job?> DequeueAsync()
    {
        var jobId = await _db.ListLeftPopAsync(JobQueueKey);
        if (jobId.IsNull)
            return null;

        var json = await _db.StringGetAsync($"{JobKey}{jobId}");
        if (json.IsNull)
            return null;

        return JsonSerializer.Deserialize<Job>(json.ToString());
    }

    public async Task RequeueAsync(Job job)
    {
        var json = JsonSerializer.Serialize(job);
        await _db.StringSetAsync($"{JobKey}{job.Id}", json);
        await _db.ListRightPushAsync(JobQueueKey, job.Id.ToString());
    }

    public async Task<int> GetCountAsync()
    {
        return (int)await _db.ListLengthAsync(JobQueueKey);
    }
}
