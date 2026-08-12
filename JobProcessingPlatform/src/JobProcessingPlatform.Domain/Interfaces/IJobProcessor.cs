namespace JobProcessingPlatform.Domain.Interfaces;

public interface IJobProcessor
{
    Task<bool> ProcessAsync(Entities.Job job, CancellationToken cancellationToken);
    string JobType { get; }
}
