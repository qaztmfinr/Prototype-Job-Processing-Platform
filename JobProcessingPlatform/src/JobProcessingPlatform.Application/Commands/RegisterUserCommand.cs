namespace JobProcessingPlatform.Application.Commands;

public record RegisterUserCommand(string Username, string Email, string Password);
