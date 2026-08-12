namespace JobProcessingPlatform.Domain.Interfaces;

public interface IUserRepository
{
    Task<Entities.User?> GetByIdAsync(Guid id);
    Task<Entities.User?> GetByUsernameAsync(string username);
    Task<Entities.User?> GetByEmailAsync(string email);
    Task AddAsync(Entities.User user);
    Task UpdateAsync(Entities.User user);
    Task DeleteAsync(Guid id);
}
