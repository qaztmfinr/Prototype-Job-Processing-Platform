using Microsoft.AspNetCore.Mvc;
using JobProcessingPlatform.Application.Handlers;
using JobProcessingPlatform.Domain.Interfaces;

namespace JobProcessingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IJobQueue _jobQueue;
    private readonly IJobRepository _jobRepository;

    public StatsController(IJobQueue jobQueue, IJobRepository jobRepository)
    {
        _jobQueue = jobQueue;
        _jobRepository = jobRepository;
    }

    /// <summary>
    /// Get queue statistics
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var handler = new GetQueueStatsQueryHandler(_jobQueue, _jobRepository);
        var stats = await handler.HandleAsync(new Application.Queries.GetQueueStatsQuery());
        return Ok(stats);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
