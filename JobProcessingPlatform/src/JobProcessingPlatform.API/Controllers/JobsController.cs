using Microsoft.AspNetCore.Mvc;
using JobProcessingPlatform.Application.Commands;
using JobProcessingPlatform.Application.Queries;
using JobProcessingPlatform.Application.Handlers;
using JobProcessingPlatform.Domain.Interfaces;
using System.Security.Claims;

namespace JobProcessingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueue _jobQueue;

    public JobsController(IJobRepository jobRepository, IJobQueue jobQueue)
    {
        _jobRepository = jobRepository;
        _jobQueue = jobQueue;
    }

    /// <summary>
    /// Create a new job
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobCommand command)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User ID not found in token");

            var handler = new CreateJobCommandHandler(_jobRepository, _jobQueue);
            var jobId = await handler.HandleAsync(new CreateJobCommand(
                command.Name,
                command.Description,
                command.PayloadJson,
                userGuid,
                command.Priority,
                command.MaxRetries,
                command.InitialDelaySeconds));

            return CreatedAtAction(nameof(GetJob), new { id = jobId }, new { id = jobId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get a job by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid id)
    {
        try
        {
            var handler = new GetJobQueryHandler(_jobRepository);
            var job = await handler.HandleAsync(new GetJobQuery(id));
            return Ok(job);
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all jobs with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs([FromQuery] int skip = 0, [FromQuery] int take = 10, [FromQuery] string? status = null)
    {
        var handler = new GetJobsQueryHandler(_jobRepository);
        var jobs = await handler.HandleAsync(new GetJobsQuery(skip, take, status));
        return Ok(jobs);
    }

    /// <summary>
    /// Cancel a job
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelJob(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized("User ID not found in token");

            var handler = new CancelJobCommandHandler(_jobRepository);
            await handler.HandleAsync(new CancelJobCommand(id, userGuid));
            return NoContent();
        }
        catch (Application.Exceptions.NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Application.Exceptions.UnauthorizedException ex)
        {
            return Forbid();
        }
    }
}
