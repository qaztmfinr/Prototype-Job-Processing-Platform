namespace JobProcessingPlatform.Application.Commands;

public record CancelJobCommand(Guid JobId, Guid RequestedBy);
