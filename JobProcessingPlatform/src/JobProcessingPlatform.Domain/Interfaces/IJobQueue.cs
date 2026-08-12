namespace JobProcessingPlatform.Domain.Interfaces;

public interface IJobQueue
{
    Task EnqueueAsync(Entities.Job job);
    Task<Entities.Job?> DequeueAsync();
    Task RequeueAsync(Entities.Job job);
    Task<int> GetCountAsync();
}
