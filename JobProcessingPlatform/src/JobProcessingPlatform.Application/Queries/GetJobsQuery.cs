namespace JobProcessingPlatform.Application.Queries;

public record GetJobsQuery(int Skip = 0, int Take = 10, string? Status = null);
