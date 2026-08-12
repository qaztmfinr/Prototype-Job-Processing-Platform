namespace JobProcessingPlatform.Domain.Interfaces;

public interface IJobRepository
{
    Task<Entities.Job?> GetByIdAsync(Guid id);
    Task<IEnumerable<Entities.Job>> GetPendingJobsAsync(int limit = 100);
    Task<IEnumerable<Entities.Job>> GetRetryingJobsAsync(int limit = 100);
    Task<IEnumerable<Entities.Job>> GetByStatusAsync(Enums.JobStatus status, int skip = 0, int take = 10);
    Task<IEnumerable<Entities.Job>> GetByCreatedByAsync(Guid userId, int skip = 0, int take = 10);
    Task AddAsync(Entities.Job job);
    Task UpdateAsync(Entities.Job job);
    Task DeleteAsync(Guid id);
    Task<int> GetCountByStatusAsync(Enums.JobStatus status);
}
