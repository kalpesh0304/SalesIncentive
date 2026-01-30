using Dorise.Incentive.Application.DataTransfer.DTOs;
using Dorise.Incentive.Application.DataTransfer.Services;
using Dorise.Incentive.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for data import and export operations.
/// "I choo-choo-choose you!" - Choose to import or export your data!
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataTransferController : ControllerBase
{
    private readonly IDataImportService _importService;
    private readonly IDataExportService _exportService;
    private readonly IDataTransferTemplateService _templateService;
    private readonly IDataTransferStatisticsService _statisticsService;
    private readonly ILogger<DataTransferController> _logger;

    public DataTransferController(
        IDataImportService importService,
        IDataExportService exportService,
        IDataTransferTemplateService templateService,
        IDataTransferStatisticsService statisticsService,
        ILogger<DataTransferController> logger)
    {
        _importService = importService;
        _exportService = exportService;
        _templateService = templateService;
        _statisticsService = statisticsService;
        _logger = logger;
    }

    // ==================== Import Endpoints ====================

    /// <summary>
    /// Upload a file for import.
    /// </summary>
    [HttpPost("imports/upload")]
    [Authorize(Roles = "Admin,Manager")]
    [RequestSizeLimit(52428800)] // 50 MB
    public async Task<ActionResult<FileUploadResponse>> UploadFile(
        IFormFile file,
        [FromForm] string importName,
        [FromForm] ImportType importType,
        [FromForm] Guid? templateId = null,
        [FromForm] bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var request = new CreateImportRequest
        {
            ImportName = importName,
            ImportType = importType,
            TemplateId = templateId,
            DryRun = dryRun
        };

        await using var stream = file.OpenReadStream();
        var result = await _importService.UploadFileAsync(stream, file.FileName, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validate an uploaded import.
    /// </summary>
    [HttpPost("imports/{importId}/validate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ImportValidationResponse>> ValidateImport(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.ValidateImportAsync(importId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Start processing an import.
    /// </summary>
    [HttpPost("imports/{importId}/start")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DataImportDto>> StartImport(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.StartImportAsync(importId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get import by ID.
    /// </summary>
    [HttpGet("imports/{importId}")]
    public async Task<ActionResult<DataImportDto>> GetImport(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetImportByIdAsync(importId, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Get import result.
    /// </summary>
    [HttpGet("imports/{importId}/result")]
    public async Task<ActionResult<ImportResultDto>> GetImportResult(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetImportResultAsync(importId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an import.
    /// </summary>
    [HttpPost("imports/{importId}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DataImportDto>> CancelImport(
        Guid importId,
        [FromBody] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.CancelImportAsync(importId, reason, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get list of imports with filtering.
    /// </summary>
    [HttpGet("imports")]
    public async Task<ActionResult<IReadOnlyList<DataImportDto>>> GetImports(
        [FromQuery] ImportType? importType = null,
        [FromQuery] ImportStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetImportsAsync(
            importType, status, fromDate, toDate, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get recent imports.
    /// </summary>
    [HttpGet("imports/recent")]
    public async Task<ActionResult<IReadOnlyList<DataImportDto>>> GetRecentImports(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetRecentImportsAsync(count, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get available fields for import type.
    /// </summary>
    [HttpGet("imports/fields/{importType}")]
    public async Task<ActionResult<EntityFieldsDto>> GetImportFields(
        ImportType importType,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.GetImportFieldsAsync(importType, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Suggest field mappings based on source columns.
    /// </summary>
    [HttpPost("imports/suggest-mappings")]
    public async Task<ActionResult<List<ImportFieldMappingDto>>> SuggestMappings(
        [FromQuery] ImportType importType,
        [FromBody] List<string> sourceColumns,
        CancellationToken cancellationToken = default)
    {
        var result = await _importService.SuggestMappingsAsync(importType, sourceColumns, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Download error report for an import.
    /// </summary>
    [HttpGet("imports/{importId}/error-report")]
    public async Task<IActionResult> DownloadErrorReport(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var report = await _importService.GetImportErrorReportAsync(importId, cancellationToken);
        return File(report, "text/csv", $"import-errors-{importId}.csv");
    }

    // ==================== Export Endpoints ====================

    /// <summary>
    /// Create a new export.
    /// </summary>
    [HttpPost("exports")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DataExportDto>> CreateExport(
        [FromBody] CreateExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.CreateExportAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Start processing an export.
    /// </summary>
    [HttpPost("exports/{exportId}/start")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DataExportDto>> StartExport(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.StartExportAsync(exportId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get export by ID.
    /// </summary>
    [HttpGet("exports/{exportId}")]
    public async Task<ActionResult<DataExportDto>> GetExport(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.GetExportByIdAsync(exportId, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Get export result.
    /// </summary>
    [HttpGet("exports/{exportId}/result")]
    public async Task<ActionResult<ExportResultDto>> GetExportResult(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.GetExportResultAsync(exportId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an export.
    /// </summary>
    [HttpPost("exports/{exportId}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DataExportDto>> CancelExport(
        Guid exportId,
        [FromBody] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.CancelExportAsync(exportId, reason, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get list of exports with filtering.
    /// </summary>
    [HttpGet("exports")]
    public async Task<ActionResult<IReadOnlyList<DataExportDto>>> GetExports(
        [FromQuery] ExportType? exportType = null,
        [FromQuery] ExportStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.GetExportsAsync(
            exportType, status, fromDate, toDate, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get recent exports.
    /// </summary>
    [HttpGet("exports/recent")]
    public async Task<ActionResult<IReadOnlyList<DataExportDto>>> GetRecentExports(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.GetRecentExportsAsync(count, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Download an export file.
    /// </summary>
    [HttpGet("exports/{exportId}/download")]
    public async Task<IActionResult> DownloadExport(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var export = await _exportService.GetExportByIdAsync(exportId, cancellationToken);
        if (export == null)
            return NotFound();

        if (!export.IsDownloadAvailable)
            return BadRequest("Export is not available for download");

        var stream = await _exportService.DownloadExportAsync(exportId, cancellationToken);
        if (stream == null)
            return NotFound("Export file not found");

        var contentType = SupportedFileFormats.GetContentType(export.FileFormat);
        return File(stream, contentType, export.FileName);
    }

    /// <summary>
    /// Refresh download URL for an export.
    /// </summary>
    [HttpPost("exports/{exportId}/refresh-url")]
    public async Task<ActionResult<string>> RefreshDownloadUrl(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.RefreshDownloadUrlAsync(exportId, cancellationToken);
        return Ok(new { downloadUrl = result });
    }

    /// <summary>
    /// Get available fields for export type.
    /// </summary>
    [HttpGet("exports/fields/{exportType}")]
    public async Task<ActionResult<EntityFieldsDto>> GetExportFields(
        ExportType exportType,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.GetExportFieldsAsync(exportType, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get default columns for export type.
    /// </summary>
    [HttpGet("exports/columns/{exportType}")]
    public async Task<ActionResult<List<ExportColumnRequest>>> GetDefaultColumns(
        ExportType exportType,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.GetDefaultColumnsAsync(exportType, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Preview export data.
    /// </summary>
    [HttpPost("exports/preview")]
    public async Task<ActionResult<List<Dictionary<string, object?>>>> PreviewExport(
        [FromBody] CreateExportRequest request,
        [FromQuery] int maxRows = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _exportService.PreviewExportAsync(request, maxRows, cancellationToken);
        return Ok(result);
    }

    // ==================== Template Endpoints ====================

    /// <summary>
    /// Create a new template.
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DataTransferTemplateDto>> CreateTemplate(
        [FromBody] DataTransferTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.CreateTemplateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update a template.
    /// </summary>
    [HttpPut("templates/{templateId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DataTransferTemplateDto>> UpdateTemplate(
        Guid templateId,
        [FromBody] DataTransferTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.UpdateTemplateAsync(templateId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a template.
    /// </summary>
    [HttpDelete("templates/{templateId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        await _templateService.DeleteTemplateAsync(templateId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get template by ID.
    /// </summary>
    [HttpGet("templates/{templateId}")]
    public async Task<ActionResult<DataTransferTemplateDto>> GetTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplateByIdAsync(templateId, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Get list of templates.
    /// </summary>
    [HttpGet("templates")]
    public async Task<ActionResult<IReadOnlyList<DataTransferTemplateDto>>> GetTemplates(
        [FromQuery] TransferDirection? direction = null,
        [FromQuery] string? entityType = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetTemplatesAsync(direction, entityType, isActive, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get default template for direction and entity type.
    /// </summary>
    [HttpGet("templates/default")]
    public async Task<ActionResult<DataTransferTemplateDto>> GetDefaultTemplate(
        [FromQuery] TransferDirection direction,
        [FromQuery] string entityType,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.GetDefaultTemplateAsync(direction, entityType, cancellationToken);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Set a template as default.
    /// </summary>
    [HttpPost("templates/{templateId}/set-default")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetDefaultTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        await _templateService.SetDefaultTemplateAsync(templateId, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Activate a template.
    /// </summary>
    [HttpPost("templates/{templateId}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ActivateTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        await _templateService.ActivateTemplateAsync(templateId, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Deactivate a template.
    /// </summary>
    [HttpPost("templates/{templateId}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateTemplate(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        await _templateService.DeactivateTemplateAsync(templateId, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Duplicate a template.
    /// </summary>
    [HttpPost("templates/{templateId}/duplicate")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DataTransferTemplateDto>> DuplicateTemplate(
        Guid templateId,
        [FromBody] string newName,
        CancellationToken cancellationToken = default)
    {
        var result = await _templateService.DuplicateTemplateAsync(templateId, newName, cancellationToken);
        return Ok(result);
    }

    // ==================== Statistics Endpoints ====================

    /// <summary>
    /// Get data transfer statistics.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<DataTransferStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsService.GetStatisticsAsync(fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get imports by type.
    /// </summary>
    [HttpGet("statistics/imports-by-type")]
    public async Task<ActionResult<Dictionary<string, int>>> GetImportsByType(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsService.GetImportsByTypeAsync(fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get exports by type.
    /// </summary>
    [HttpGet("statistics/exports-by-type")]
    public async Task<ActionResult<Dictionary<string, int>>> GetExportsByType(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsService.GetExportsByTypeAsync(fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get recent transfers.
    /// </summary>
    [HttpGet("statistics/recent")]
    public async Task<ActionResult<List<RecentTransferDto>>> GetRecentTransfers(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsService.GetRecentTransfersAsync(count, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get transfer volume trend.
    /// </summary>
    [HttpGet("statistics/trend")]
    public async Task<ActionResult<Dictionary<string, long>>> GetTransferVolumeTrend(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _statisticsService.GetTransferVolumeTrendAsync(days, cancellationToken);
        return Ok(result);
    }

    // ==================== Supported Formats ====================

    /// <summary>
    /// Get supported file formats.
    /// </summary>
    [HttpGet("formats")]
    public IActionResult GetSupportedFormats()
    {
        return Ok(new
        {
            ImportFormats = SupportedFileFormats.ImportFormats,
            ExportFormats = SupportedFileFormats.ExportFormats
        });
    }
}
