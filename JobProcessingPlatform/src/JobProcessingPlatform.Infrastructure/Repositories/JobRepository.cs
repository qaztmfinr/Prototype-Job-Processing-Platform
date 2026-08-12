using JobProcessingPlatform.Domain.Entities;
using JobProcessingPlatform.Domain.Enums;
using JobProcessingPlatform.Domain.Interfaces;

namespace JobProcessingPlatform.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly Persistence.JobProcessingDbContext _context;

    public JobRepository(Persistence.JobProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _context.Jobs.FindAsync(id);
    }

    public async Task<IEnumerable<Job>> GetPendingJobsAsync(int limit = 100)
    {
        return await Task.FromResult(_context.Jobs
            .Where(j => j.Status == JobStatus.Pending)
            .OrderBy(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .Take(limit)
            .ToList());
    }

    public async Task<IEnumerable<Job>> GetRetryingJobsAsync(int limit = 100)
    {
        return await Task.FromResult(_context.Jobs
            .Where(j => j.Status == JobStatus.Retrying)
            .OrderBy(j => j.Priority)
            .Take(limit)
            .ToList());
    }

    public async Task<IEnumerable<Job>> GetByStatusAsync(JobStatus status, int skip = 0, int take = 10)
    {
        return await Task.FromResult(_context.Jobs
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList());
    }

    public async Task<IEnumerable<Job>> GetByCreatedByAsync(Guid userId, int skip = 0, int take = 10)
    {
        return await Task.FromResult(_context.Jobs
            .Where(j => j.CreatedBy == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList());
    }

    public async Task AddAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job job)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await GetByIdAsync(id);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetCountByStatusAsync(JobStatus status)
    {
        return await Task.FromResult(_context.Jobs.Count(j => j.Status == status));
    }
}
