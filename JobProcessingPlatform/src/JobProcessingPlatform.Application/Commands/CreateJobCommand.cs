using JobProcessingPlatform.Domain.Enums;

namespace JobProcessingPlatform.Application.Commands;

public record CreateJobCommand(
    string Name,
    string Description,
    string PayloadJson,
    Guid CreatedBy,
    JobPriority Priority = JobPriority.Normal,
    int? MaxRetries = null,
    int? InitialDelaySeconds = null);
