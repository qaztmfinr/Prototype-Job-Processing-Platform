namespace JobProcessingPlatform.Application.Services;

public interface ITokenService
{
    string GenerateToken(Guid userId, string username, string role);
    bool ValidateToken(string token);
}
