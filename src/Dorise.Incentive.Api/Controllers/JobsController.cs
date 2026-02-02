using Asp.Versioning;
using Dorise.Incentive.Application.Jobs.DTOs;
using Dorise.Incentive.Application.Jobs.Services;
using Dorise.Incentive.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for background job management.
/// "I choo-choo-choose you!" - Choose your jobs wisely!
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IJobScheduleService _scheduleService;
    private readonly IBatchOperationService _batchService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobService jobService,
        IJobScheduleService scheduleService,
        IBatchOperationService batchService,
        ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _scheduleService = scheduleService;
        _batchService = batchService;
        _logger = logger;
    }

    // ============== Jobs ==============

    /// <summary>
    /// Get job by ID.
    /// </summary>
    [HttpGet("{jobId:guid}")]
    [ProducesResponseType(typeof(BackgroundJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobByIdAsync(jobId, cancellationToken);
        return job == null ? NotFound() : Ok(job);
    }

    /// <summary>
    /// Search jobs.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchJobs(
        [FromQuery] JobSearchQuery query,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetJobsAsync(query, cancellationToken);
        return Ok(jobs);
    }

    /// <summary>
    /// Get recent jobs.
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentJobs(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _jobService.GetRecentJobsAsync(count, cancellationToken);
        return Ok(jobs);
    }

    /// <summary>
    /// Get jobs by correlation ID.
    /// </summary>
    [HttpGet("correlation/{correlationId}")]
    [ProducesResponseType(typeof(IReadOnlyList<BackgroundJobDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobsByCorrelationId(
        string correlationId,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobService.GetJobsByCorrelationIdAsync(correlationId, cancellationToken);
        return Ok(jobs);
    }

    /// <summary>
    /// Create a new job.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(BackgroundJobDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateJob(
        [FromBody] CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        var job = await _jobService.CreateJobAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetJob), new { jobId = job.Id }, job);
    }

    /// <summary>
    /// Cancel a job.
    /// </summary>
    [HttpPost("{jobId:guid}/cancel")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(BackgroundJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelJob(
        Guid jobId,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var job = await _jobService.CancelJobAsync(jobId, reason, cancellationToken);
            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Retry a failed job.
    /// </summary>
    [HttpPost("{jobId:guid}/retry")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(BackgroundJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RetryJob(Guid jobId, CancellationToken cancellationToken)
    {
        try
        {
            var job = await _jobService.RetryJobAsync(jobId, cancellationToken);
            return Ok(job);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get queue status.
    /// </summary>
    [HttpGet("queue/status")]
    [ProducesResponseType(typeof(JobQueueStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQueueStatus(CancellationToken cancellationToken)
    {
        var status = await _jobService.GetQueueStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Get job statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(JobStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var stats = await _jobService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Cleanup completed jobs.
    /// </summary>
    [HttpPost("cleanup")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupJobs(
        [FromQuery] int olderThanDays = 30,
        CancellationToken cancellationToken = default)
    {
        var count = await _jobService.CleanupCompletedJobsAsync(olderThanDays, cancellationToken);
        return Ok(new { CleanedUp = count });
    }

    // ============== Schedules ==============

    /// <summary>
    /// Get all schedules.
    /// </summary>
    [HttpGet("schedules")]
    [ProducesResponseType(typeof(IReadOnlyList<JobScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSchedules(CancellationToken cancellationToken)
    {
        var schedules = await _scheduleService.GetAllSchedulesAsync(cancellationToken);
        return Ok(schedules);
    }

    /// <summary>
    /// Get enabled schedules.
    /// </summary>
    [HttpGet("schedules/enabled")]
    [ProducesResponseType(typeof(IReadOnlyList<JobScheduleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnabledSchedules(CancellationToken cancellationToken)
    {
        var schedules = await _scheduleService.GetEnabledSchedulesAsync(cancellationToken);
        return Ok(schedules);
    }

    /// <summary>
    /// Get schedule by ID.
    /// </summary>
    [HttpGet("schedules/{scheduleId:guid}")]
    [ProducesResponseType(typeof(JobScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleService.GetScheduleByIdAsync(scheduleId, cancellationToken);
        return schedule == null ? NotFound() : Ok(schedule);
    }

    /// <summary>
    /// Create a new schedule.
    /// </summary>
    [HttpPost("schedules")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(JobScheduleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] CreateJobScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _scheduleService.CreateScheduleAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSchedule), new { scheduleId = schedule.Id }, schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update a schedule.
    /// </summary>
    [HttpPut("schedules/{scheduleId:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(JobScheduleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSchedule(
        Guid scheduleId,
        [FromBody] UpdateJobScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _scheduleService.UpdateScheduleAsync(scheduleId, request, cancellationToken);
            return Ok(schedule);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a schedule.
    /// </summary>
    [HttpDelete("schedules/{scheduleId:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        try
        {
            await _scheduleService.DeleteScheduleAsync(scheduleId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Enable a schedule.
    /// </summary>
    [HttpPost("schedules/{scheduleId:guid}/enable")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        try
        {
            await _scheduleService.EnableScheduleAsync(scheduleId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Disable a schedule.
    /// </summary>
    [HttpPost("schedules/{scheduleId:guid}/disable")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        try
        {
            await _scheduleService.DisableScheduleAsync(scheduleId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Toggle a schedule.
    /// </summary>
    [HttpPost("schedules/{scheduleId:guid}/toggle")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        try
        {
            var isEnabled = await _scheduleService.ToggleScheduleAsync(scheduleId, cancellationToken);
            return Ok(new { IsEnabled = isEnabled });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Trigger a schedule immediately.
    /// </summary>
    [HttpPost("schedules/{scheduleId:guid}/trigger")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(BackgroundJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriggerSchedule(Guid scheduleId, CancellationToken cancellationToken)
    {
        try
        {
            var job = await _scheduleService.TriggerScheduleNowAsync(scheduleId, cancellationToken);
            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    // ============== Batch Operations ==============

    /// <summary>
    /// Start a calculation batch.
    /// </summary>
    [HttpPost("batch/calculations")]
    [Authorize(Roles = "Administrator,Manager")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> StartCalculationBatch(
        [FromBody] CalculationBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.StartCalculationBatchAsync(request, cancellationToken);
        return AcceptedAtAction(nameof(GetJob), new { jobId = result.JobId }, result);
    }

    /// <summary>
    /// Get calculation batch result.
    /// </summary>
    [HttpGet("batch/calculations/{jobId:guid}")]
    [ProducesResponseType(typeof(CalculationBatchResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCalculationBatchResult(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.GetCalculationBatchResultAsync(jobId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Start a data export.
    /// </summary>
    [HttpPost("batch/export")]
    [Authorize(Roles = "Administrator,Manager,Analyst")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> StartDataExport(
        [FromBody] DataExportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.StartDataExportAsync(request, cancellationToken);
        return AcceptedAtAction(nameof(GetJob), new { jobId = result.JobId }, result);
    }

    /// <summary>
    /// Get data export result.
    /// </summary>
    [HttpGet("batch/export/{jobId:guid}")]
    [ProducesResponseType(typeof(DataExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDataExportResult(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.GetDataExportResultAsync(jobId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Start a data import.
    /// </summary>
    [HttpPost("batch/import")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> StartDataImport(
        [FromBody] DataImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.StartDataImportAsync(request, cancellationToken);
        return AcceptedAtAction(nameof(GetJob), new { jobId = result.JobId }, result);
    }

    /// <summary>
    /// Get data import result.
    /// </summary>
    [HttpGet("batch/import/{jobId:guid}")]
    [ProducesResponseType(typeof(DataImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDataImportResult(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.GetDataImportResultAsync(jobId, cancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Start a generic batch operation.
    /// </summary>
    [HttpPost("batch/custom")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BatchOperationResult), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> StartBatchOperation(
        [FromBody] BatchOperationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _batchService.StartBatchOperationAsync(request, cancellationToken);
        return AcceptedAtAction(nameof(GetJob), new { jobId = result.JobId }, result);
    }
}
