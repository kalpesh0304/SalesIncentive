using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Queries;

namespace Dorise.Incentive.Application.Reports.Services;

/// <summary>
/// Service interface for exporting reports to various formats.
/// "That's where I saw the leprechaun!" - Export your data like magic!
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports payout report to the specified format.
    /// </summary>
    Task<ExportResultDto> ExportPayoutReportAsync(
        PayoutReportDto report,
        ExportFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports achievement summary to the specified format.
    /// </summary>
    Task<ExportResultDto> ExportAchievementSummaryAsync(
        AchievementSummaryDto report,
        ExportFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports variance analysis to the specified format.
    /// </summary>
    Task<ExportResultDto> ExportVarianceAnalysisAsync(
        VarianceAnalysisDto report,
        ExportFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports generic data to CSV format.
    /// </summary>
    Task<ExportResultDto> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports generic data to Excel format.
    /// </summary>
    Task<ExportResultDto> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string fileName,
        string sheetName = "Data",
        CancellationToken cancellationToken = default);
}
