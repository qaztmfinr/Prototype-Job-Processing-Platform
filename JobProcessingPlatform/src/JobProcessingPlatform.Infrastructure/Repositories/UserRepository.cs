using JobProcessingPlatform.Domain.Entities;
using JobProcessingPlatform.Domain.Interfaces;

namespace JobProcessingPlatform.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly Persistence.JobProcessingDbContext _context;

    public UserRepository(Persistence.JobProcessingDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Task.FromResult(_context.Users
            .FirstOrDefault(u => u.Username == username));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Task.FromResult(_context.Users
            .FirstOrDefault(u => u.Email == email));
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
