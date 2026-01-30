using System.Reflection;
using System.Text;
using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Queries;
using Dorise.Incentive.Application.Reports.Services;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Services;

/// <summary>
/// Implementation of export service for generating reports in various formats.
/// "I'm Idaho!" - And I export data in many formats!
/// </summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
    }

    public Task<ExportResultDto> ExportPayoutReportAsync(
        PayoutReportDto report,
        ExportFormat format,
        CancellationToken cancellationToken = default)
    {
        return format switch
        {
            ExportFormat.Csv => ExportPayoutToCsvAsync(report, cancellationToken),
            ExportFormat.Excel => ExportPayoutToExcelAsync(report, cancellationToken),
            ExportFormat.Pdf => ExportPayoutToPdfAsync(report, cancellationToken),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    public Task<ExportResultDto> ExportAchievementSummaryAsync(
        AchievementSummaryDto report,
        ExportFormat format,
        CancellationToken cancellationToken = default)
    {
        return format switch
        {
            ExportFormat.Csv => ExportAchievementToCsvAsync(report, cancellationToken),
            ExportFormat.Excel => ExportAchievementToExcelAsync(report, cancellationToken),
            ExportFormat.Pdf => ExportAchievementToPdfAsync(report, cancellationToken),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    public Task<ExportResultDto> ExportVarianceAnalysisAsync(
        VarianceAnalysisDto report,
        ExportFormat format,
        CancellationToken cancellationToken = default)
    {
        return format switch
        {
            ExportFormat.Csv => ExportVarianceToCsvAsync(report, cancellationToken),
            ExportFormat.Excel => ExportVarianceToExcelAsync(report, cancellationToken),
            ExportFormat.Pdf => ExportVarianceToPdfAsync(report, cancellationToken),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    public Task<ExportResultDto> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var csv = GenerateCsv(data);
        var content = Encoding.UTF8.GetBytes(csv);

        _logger.LogInformation("Exported {Count} records to CSV: {FileName}",
            data.Count(), fileName);

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"{fileName}.csv",
            ContentType = "text/csv",
            Content = content,
            RecordCount = data.Count(),
            GeneratedAt = DateTime.UtcNow
        });
    }

    public Task<ExportResultDto> ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string fileName,
        string sheetName = "Data",
        CancellationToken cancellationToken = default)
    {
        // For a real implementation, use a library like EPPlus or ClosedXML
        // This is a simplified implementation that generates CSV formatted as Excel
        var csv = GenerateCsv(data);
        var content = Encoding.UTF8.GetBytes(csv);

        _logger.LogInformation("Exported {Count} records to Excel: {FileName}",
            data.Count(), fileName);

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"{fileName}.xlsx",
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Content = content,
            RecordCount = data.Count(),
            GeneratedAt = DateTime.UtcNow
        });
    }

    private Task<ExportResultDto> ExportPayoutToCsvAsync(
        PayoutReportDto report,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        // Header section
        sb.AppendLine("PAYOUT REPORT");
        sb.AppendLine($"Period: {report.Period}");
        sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Summary section
        sb.AppendLine("SUMMARY");
        sb.AppendLine($"Total Employees,{report.Summary.TotalEmployees}");
        sb.AppendLine($"Eligible Employees,{report.Summary.EligibleEmployees}");
        sb.AppendLine($"Paid Employees,{report.Summary.PaidEmployees}");
        sb.AppendLine($"Total Gross Incentive,{report.Summary.TotalGrossIncentive:N2}");
        sb.AppendLine($"Total Net Incentive,{report.Summary.TotalNetIncentive:N2}");
        sb.AppendLine($"Average Incentive,{report.Summary.AverageIncentive:N2}");
        sb.AppendLine();

        // Details section
        sb.AppendLine("DETAILS");
        sb.AppendLine("Employee Code,Employee Name,Department,Plan,Target,Actual,Achievement %,Gross Incentive,Net Incentive,Status");

        foreach (var detail in report.Details)
        {
            sb.AppendLine($"{detail.EmployeeCode},{EscapeCsv(detail.EmployeeName)},{EscapeCsv(detail.Department)},{EscapeCsv(detail.PlanName)},{detail.TargetValue:N2},{detail.ActualValue:N2},{detail.AchievementPercentage:N2},{detail.GrossIncentive:N2},{detail.NetIncentive:N2},{detail.Status}");
        }

        var content = Encoding.UTF8.GetBytes(sb.ToString());

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"PayoutReport_{report.Period.Replace(" to ", "_")}.csv",
            ContentType = "text/csv",
            Content = content,
            RecordCount = report.Details.Count,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private Task<ExportResultDto> ExportPayoutToExcelAsync(
        PayoutReportDto report,
        CancellationToken cancellationToken)
    {
        // For production, use EPPlus or ClosedXML to generate proper Excel files
        // This implementation returns CSV content with Excel headers for demo purposes
        return ExportPayoutToCsvAsync(report, cancellationToken).ContinueWith(t =>
        {
            var result = t.Result;
            return result with
            {
                FileName = $"PayoutReport_{report.Period.Replace(" to ", "_")}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }, cancellationToken);
    }

    private Task<ExportResultDto> ExportPayoutToPdfAsync(
        PayoutReportDto report,
        CancellationToken cancellationToken)
    {
        // For production, use a PDF library like iTextSharp or QuestPDF
        // This is a placeholder that returns a simple text representation
        var sb = new StringBuilder();
        sb.AppendLine("PAYOUT REPORT");
        sb.AppendLine($"Period: {report.Period}");
        sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine($"Total Net Incentive: {report.Summary.Currency} {report.Summary.TotalNetIncentive:N2}");
        sb.AppendLine($"Total Employees: {report.Summary.TotalEmployees}");

        var content = Encoding.UTF8.GetBytes(sb.ToString());

        _logger.LogWarning("PDF export is using placeholder implementation");

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"PayoutReport_{report.Period.Replace(" to ", "_")}.pdf",
            ContentType = "application/pdf",
            Content = content,
            RecordCount = report.Details.Count,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private Task<ExportResultDto> ExportAchievementToCsvAsync(
        AchievementSummaryDto report,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("ACHIEVEMENT SUMMARY REPORT");
        sb.AppendLine($"Period: {report.Period}");
        sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Overall section
        sb.AppendLine("OVERALL STATISTICS");
        sb.AppendLine($"Total Employees,{report.Overall.TotalEmployees}");
        sb.AppendLine($"Average Achievement,{report.Overall.AverageAchievement:N2}%");
        sb.AppendLine($"Above Target,{report.Overall.AboveTargetCount}");
        sb.AppendLine($"At Target,{report.Overall.AtTargetCount}");
        sb.AppendLine($"Below Target,{report.Overall.BelowTargetCount}");
        sb.AppendLine();

        // By Department
        sb.AppendLine("BY DEPARTMENT");
        sb.AppendLine("Department,Employees,Avg Achievement,Total Target,Total Actual,Total Incentive");
        foreach (var dept in report.ByDepartment)
        {
            sb.AppendLine($"{EscapeCsv(dept.DepartmentName)},{dept.EmployeeCount},{dept.AverageAchievement:N2}%,{dept.TotalTarget:N2},{dept.TotalActual:N2},{dept.TotalIncentive:N2}");
        }
        sb.AppendLine();

        // By Plan
        sb.AppendLine("BY PLAN");
        sb.AppendLine("Plan Code,Plan Name,Employees,Avg Achievement,Total Incentive");
        foreach (var plan in report.ByPlan)
        {
            sb.AppendLine($"{plan.PlanCode},{EscapeCsv(plan.PlanName)},{plan.EmployeeCount},{plan.AverageAchievement:N2}%,{plan.TotalIncentive:N2}");
        }
        sb.AppendLine();

        // By Achievement Band
        sb.AppendLine("BY ACHIEVEMENT BAND");
        sb.AppendLine("Band,Employee Count,Percentage");
        foreach (var band in report.ByAchievementBand)
        {
            sb.AppendLine($"{band.BandName},{band.EmployeeCount},{band.Percentage:N2}%");
        }

        var content = Encoding.UTF8.GetBytes(sb.ToString());

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"AchievementSummary_{report.Period.Replace(" to ", "_")}.csv",
            ContentType = "text/csv",
            Content = content,
            RecordCount = report.ByDepartment.Count + report.ByPlan.Count,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private Task<ExportResultDto> ExportAchievementToExcelAsync(
        AchievementSummaryDto report,
        CancellationToken cancellationToken)
    {
        return ExportAchievementToCsvAsync(report, cancellationToken).ContinueWith(t =>
        {
            var result = t.Result;
            return result with
            {
                FileName = $"AchievementSummary_{report.Period.Replace(" to ", "_")}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }, cancellationToken);
    }

    private Task<ExportResultDto> ExportAchievementToPdfAsync(
        AchievementSummaryDto report,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ACHIEVEMENT SUMMARY REPORT");
        sb.AppendLine($"Period: {report.Period}");
        sb.AppendLine($"Average Achievement: {report.Overall.AverageAchievement:N2}%");

        var content = Encoding.UTF8.GetBytes(sb.ToString());

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"AchievementSummary_{report.Period.Replace(" to ", "_")}.pdf",
            ContentType = "application/pdf",
            Content = content,
            RecordCount = report.ByDepartment.Count,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private Task<ExportResultDto> ExportVarianceToCsvAsync(
        VarianceAnalysisDto report,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("VARIANCE ANALYSIS REPORT");
        sb.AppendLine($"Current Period: {report.CurrentPeriod}");
        sb.AppendLine($"Previous Period: {report.PreviousPeriod}");
        sb.AppendLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Summary
        sb.AppendLine("SUMMARY");
        sb.AppendLine($"Current Period Total,{report.Summary.CurrentPeriodTotal:N2}");
        sb.AppendLine($"Previous Period Total,{report.Summary.PreviousPeriodTotal:N2}");
        sb.AppendLine($"Absolute Variance,{report.Summary.AbsoluteVariance:N2}");
        sb.AppendLine($"Percentage Variance,{report.Summary.PercentageVariance:N2}%");
        sb.AppendLine();

        // Top Gainers
        sb.AppendLine("TOP GAINERS");
        sb.AppendLine("Employee Code,Employee Name,Department,Current,Previous,Variance,Variance %");
        foreach (var emp in report.TopGainers)
        {
            sb.AppendLine($"{emp.EmployeeCode},{EscapeCsv(emp.EmployeeName)},{EscapeCsv(emp.Department)},{emp.CurrentIncentive:N2},{emp.PreviousIncentive:N2},{emp.AbsoluteVariance:N2},{emp.PercentageVariance:N2}%");
        }
        sb.AppendLine();

        // Top Decliners
        sb.AppendLine("TOP DECLINERS");
        sb.AppendLine("Employee Code,Employee Name,Department,Current,Previous,Variance,Variance %");
        foreach (var emp in report.TopDecliners)
        {
            sb.AppendLine($"{emp.EmployeeCode},{EscapeCsv(emp.EmployeeName)},{EscapeCsv(emp.Department)},{emp.CurrentIncentive:N2},{emp.PreviousIncentive:N2},{emp.AbsoluteVariance:N2},{emp.PercentageVariance:N2}%");
        }
        sb.AppendLine();

        // By Department
        sb.AppendLine("BY DEPARTMENT");
        sb.AppendLine("Department,Current Total,Previous Total,Variance,Variance %");
        foreach (var dept in report.ByDepartment)
        {
            sb.AppendLine($"{EscapeCsv(dept.DepartmentName)},{dept.CurrentTotal:N2},{dept.PreviousTotal:N2},{dept.AbsoluteVariance:N2},{dept.PercentageVariance:N2}%");
        }

        var content = Encoding.UTF8.GetBytes(sb.ToString());

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"VarianceAnalysis_{report.CurrentPeriod}_vs_{report.PreviousPeriod}.csv",
            ContentType = "text/csv",
            Content = content,
            RecordCount = report.TopGainers.Count + report.TopDecliners.Count,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private Task<ExportResultDto> ExportVarianceToExcelAsync(
        VarianceAnalysisDto report,
        CancellationToken cancellationToken)
    {
        return ExportVarianceToCsvAsync(report, cancellationToken).ContinueWith(t =>
        {
            var result = t.Result;
            return result with
            {
                FileName = $"VarianceAnalysis_{report.CurrentPeriod}_vs_{report.PreviousPeriod}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }, cancellationToken);
    }

    private Task<ExportResultDto> ExportVarianceToPdfAsync(
        VarianceAnalysisDto report,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("VARIANCE ANALYSIS REPORT");
        sb.AppendLine($"Comparing: {report.CurrentPeriod} vs {report.PreviousPeriod}");
        sb.AppendLine($"Total Variance: {report.Summary.Currency} {report.Summary.AbsoluteVariance:N2}");

        var content = Encoding.UTF8.GetBytes(sb.ToString());

        return Task.FromResult(new ExportResultDto
        {
            FileName = $"VarianceAnalysis_{report.CurrentPeriod}_vs_{report.PreviousPeriod}.pdf",
            ContentType = "application/pdf",
            Content = content,
            RecordCount = report.ByDepartment.Count,
            GeneratedAt = DateTime.UtcNow
        });
    }

    private static string GenerateCsv<T>(IEnumerable<T> data)
    {
        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Header row
        sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return EscapeCsv(value?.ToString() ?? string.Empty);
            });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
