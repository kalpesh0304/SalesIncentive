using Dorise.Incentive.Application.DataTransfer.DTOs;
using Dorise.Incentive.Application.DataTransfer.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.DataTransfer;

/// <summary>
/// Implementation of IFileParserService for parsing and generating files.
/// "I bent my Wookie!" - Bending files into the right format!
/// </summary>
public class FileParserService : IFileParserService
{
    private readonly ILogger<FileParserService> _logger;

    private static readonly Dictionary<string, long> MaxFileSizes = new()
    {
        { "csv", 50 * 1024 * 1024 },   // 50 MB
        { "xlsx", 25 * 1024 * 1024 },  // 25 MB
        { "xls", 25 * 1024 * 1024 },   // 25 MB
        { "json", 50 * 1024 * 1024 },  // 50 MB
        { "xml", 50 * 1024 * 1024 },   // 50 MB
        { "pdf", 100 * 1024 * 1024 }   // 100 MB
    };

    public FileParserService(ILogger<FileParserService> logger)
    {
        _logger = logger;
    }

    // CSV Operations
    public async Task<List<string>> GetCsvHeadersAsync(
        Stream stream,
        string delimiter = ",",
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrEmpty(headerLine))
            return new List<string>();

        return ParseCsvLine(headerLine, delimiter[0]);
    }

    public async Task<List<Dictionary<string, string>>> ParseCsvAsync(
        Stream stream,
        string delimiter = ",",
        bool hasHeader = true,
        int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<Dictionary<string, string>>();
        using var reader = new StreamReader(stream, leaveOpen: true);

        var headers = new List<string>();
        if (hasHeader)
        {
            var headerLine = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrEmpty(headerLine))
                headers = ParseCsvLine(headerLine, delimiter[0]);
        }

        string? line;
        var rowCount = 0;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (maxRows.HasValue && rowCount >= maxRows.Value)
                break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line, delimiter[0]);
            var row = new Dictionary<string, string>();

            for (int i = 0; i < values.Count; i++)
            {
                var key = hasHeader && i < headers.Count ? headers[i] : $"Column{i + 1}";
                row[key] = values[i];
            }

            results.Add(row);
            rowCount++;
        }

        _logger.LogDebug("Parsed {RowCount} rows from CSV", rowCount);
        return results;
    }

    public Task<Stream> GenerateCsvAsync<T>(
        IEnumerable<T> data,
        List<ExportColumnRequest>? columns = null,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);

        var delimiter = options?.Delimiter ?? ",";
        var includeHeaders = options?.IncludeHeaders ?? true;

        var dataList = data.ToList();
        if (!dataList.Any())
        {
            stream.Position = 0;
            return Task.FromResult<Stream>(stream);
        }

        var properties = typeof(T).GetProperties();
        var orderedColumns = columns?.OrderBy(c => c.Order).ToList()
            ?? properties.Select((p, i) => new ExportColumnRequest
            {
                FieldName = p.Name,
                DisplayName = p.Name,
                Order = i
            }).ToList();

        // Write headers
        if (includeHeaders)
        {
            var headerLine = string.Join(delimiter,
                orderedColumns.Select(c => EscapeCsvField(c.DisplayName ?? c.FieldName)));
            writer.WriteLine(headerLine);
        }

        // Write data rows
        foreach (var item in dataList)
        {
            var values = orderedColumns.Select(col =>
            {
                var prop = properties.FirstOrDefault(p =>
                    p.Name.Equals(col.FieldName, StringComparison.OrdinalIgnoreCase));
                var value = prop?.GetValue(item);
                return FormatValue(value, col.Format);
            });

            writer.WriteLine(string.Join(delimiter, values.Select(EscapeCsvField)));
        }

        writer.Flush();
        stream.Position = 0;

        _logger.LogDebug("Generated CSV with {RowCount} rows", dataList.Count);
        return Task.FromResult<Stream>(stream);
    }

    // Excel Operations
    public Task<List<string>> GetExcelSheetsAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, use a library like EPPlus or ClosedXML
        _logger.LogDebug("Getting Excel sheets (stub implementation)");
        return Task.FromResult(new List<string> { "Sheet1" });
    }

    public Task<List<string>> GetExcelHeadersAsync(
        Stream stream,
        string? sheetName = null,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, use a library like EPPlus or ClosedXML
        _logger.LogDebug("Getting Excel headers from sheet {SheetName}", sheetName ?? "default");
        return Task.FromResult(new List<string> { "Column1", "Column2", "Column3" });
    }

    public Task<List<Dictionary<string, object?>>> ParseExcelAsync(
        Stream stream,
        string? sheetName = null,
        bool hasHeader = true,
        int? maxRows = null,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, use a library like EPPlus or ClosedXML
        _logger.LogDebug("Parsing Excel file from sheet {SheetName}", sheetName ?? "default");
        return Task.FromResult(new List<Dictionary<string, object?>>());
    }

    public Task<Stream> GenerateExcelAsync<T>(
        IEnumerable<T> data,
        List<ExportColumnRequest>? columns = null,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, use a library like EPPlus or ClosedXML
        var stream = new MemoryStream();
        _logger.LogDebug("Generating Excel file (stub implementation)");
        stream.Position = 0;
        return Task.FromResult<Stream>(stream);
    }

    // JSON Operations
    public async Task<List<Dictionary<string, object?>>> ParseJsonAsync(
        Stream stream,
        string? rootPath = null,
        int? maxItems = null,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var json = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, options)
                ?? new List<Dictionary<string, object?>>();

            if (maxItems.HasValue)
                items = items.Take(maxItems.Value).ToList();

            _logger.LogDebug("Parsed {ItemCount} items from JSON", items.Count);
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse JSON");
            throw new InvalidOperationException("Failed to parse JSON file", ex);
        }
    }

    public Task<Stream> GenerateJsonAsync<T>(
        IEnumerable<T> data,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        System.Text.Json.JsonSerializer.Serialize(stream, data, jsonOptions);
        stream.Position = 0;

        _logger.LogDebug("Generated JSON file");
        return Task.FromResult<Stream>(stream);
    }

    // XML Operations
    public Task<List<Dictionary<string, object?>>> ParseXmlAsync(
        Stream stream,
        string? rootElement = null,
        string? itemElement = null,
        int? maxItems = null,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, use XmlDocument or XDocument
        _logger.LogDebug("Parsing XML file");
        return Task.FromResult(new List<Dictionary<string, object?>>());
    }

    public Task<Stream> GenerateXmlAsync<T>(
        IEnumerable<T> data,
        string rootElement = "Data",
        string itemElement = "Item",
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);

        writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        writer.WriteLine($"<{rootElement}>");

        var properties = typeof(T).GetProperties();
        foreach (var item in data)
        {
            writer.WriteLine($"  <{itemElement}>");
            foreach (var prop in properties)
            {
                var value = prop.GetValue(item)?.ToString() ?? "";
                writer.WriteLine($"    <{prop.Name}>{System.Security.SecurityElement.Escape(value)}</{prop.Name}>");
            }
            writer.WriteLine($"  </{itemElement}>");
        }

        writer.WriteLine($"</{rootElement}>");
        writer.Flush();
        stream.Position = 0;

        _logger.LogDebug("Generated XML file");
        return Task.FromResult<Stream>(stream);
    }

    // PDF Operations
    public Task<Stream> GeneratePdfAsync<T>(
        IEnumerable<T> data,
        List<ExportColumnRequest>? columns = null,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, use a library like iTextSharp or QuestPDF
        var stream = new MemoryStream();
        _logger.LogDebug("Generating PDF file (stub implementation)");
        stream.Position = 0;
        return Task.FromResult<Stream>(stream);
    }

    // File Detection
    public string DetectFileFormat(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant();
        return extension ?? "unknown";
    }

    public bool IsValidFileFormat(string fileName, TransferDirection direction)
    {
        var format = DetectFileFormat(fileName);
        return direction == TransferDirection.Import
            ? SupportedFileFormats.IsValidImportFormat(format)
            : SupportedFileFormats.IsValidExportFormat(format);
    }

    public long GetMaxFileSize(string fileFormat)
    {
        return MaxFileSizes.TryGetValue(fileFormat.ToLowerInvariant(), out var size)
            ? size
            : 10 * 1024 * 1024; // Default 10 MB
    }

    // Helper methods
    private static List<string> ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString().Trim());
        return result;
    }

    private static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static string FormatValue(object? value, string? format)
    {
        if (value == null)
            return "";

        if (value is DateTime dt && !string.IsNullOrEmpty(format))
            return dt.ToString(format);

        if (value is decimal dec && !string.IsNullOrEmpty(format))
            return dec.ToString(format);

        if (value is double dbl && !string.IsNullOrEmpty(format))
            return dbl.ToString(format);

        return value.ToString() ?? "";
    }
}

/// <summary>
/// Implementation of IDataTransferStatisticsService.
/// "I'm a unitard!" - Unified statistics for all data transfers!
/// </summary>
public class DataTransferStatisticsService : IDataTransferStatisticsService
{
    private readonly IDataImportRepository _importRepository;
    private readonly IDataExportRepository _exportRepository;
    private readonly ILogger<DataTransferStatisticsService> _logger;

    public DataTransferStatisticsService(
        IDataImportRepository importRepository,
        IDataExportRepository exportRepository,
        ILogger<DataTransferStatisticsService> logger)
    {
        _importRepository = importRepository;
        _exportRepository = exportRepository;
        _logger = logger;
    }

    public async Task<DataTransferStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var importsByType = await GetImportsByTypeAsync(fromDate, toDate, cancellationToken);
        var exportsByType = await GetExportsByTypeAsync(fromDate, toDate, cancellationToken);
        var recentTransfers = await GetRecentTransfersAsync(10, cancellationToken);

        var totalImports = importsByType.Values.Sum();
        var totalExports = exportsByType.Values.Sum();

        var successfulImports = await _importRepository.CountByStatusAsync(
            ImportStatus.Completed, fromDate, toDate, cancellationToken);
        var failedImports = await _importRepository.CountByStatusAsync(
            ImportStatus.Failed, fromDate, toDate, cancellationToken);

        var successfulExports = await _exportRepository.CountByStatusAsync(
            ExportStatus.Completed, fromDate, toDate, cancellationToken);
        var failedExports = await _exportRepository.CountByStatusAsync(
            ExportStatus.Failed, fromDate, toDate, cancellationToken);

        return new DataTransferStatisticsDto
        {
            TotalImports = totalImports,
            SuccessfulImports = successfulImports,
            FailedImports = failedImports,
            TotalExports = totalExports,
            SuccessfulExports = successfulExports,
            FailedExports = failedExports,
            TotalRecordsImported = 0, // Would need to aggregate from completed imports
            TotalRecordsExported = 0, // Would need to aggregate from completed exports
            ImportsByType = importsByType,
            ExportsByType = exportsByType,
            RecentTransfers = recentTransfers
        };
    }

    public async Task<Dictionary<string, int>> GetImportsByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var counts = await _importRepository.GetCountByTypeAsync(fromDate, toDate, cancellationToken);
        return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
    }

    public async Task<Dictionary<string, int>> GetExportsByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var counts = await _exportRepository.GetCountByTypeAsync(fromDate, toDate, cancellationToken);
        return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
    }

    public async Task<List<RecentTransferDto>> GetRecentTransfersAsync(
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        var recentImports = await _importRepository.GetRecentAsync(count / 2, cancellationToken);
        var recentExports = await _exportRepository.GetRecentAsync(count / 2, cancellationToken);

        var transfers = new List<RecentTransferDto>();

        transfers.AddRange(recentImports.Select(i => new RecentTransferDto
        {
            Id = i.Id,
            Name = i.ImportName,
            Direction = TransferDirection.Import,
            Type = i.ImportType.ToString(),
            Status = i.Status.ToString(),
            RecordCount = i.TotalRecords,
            CreatedAt = i.CreatedAt,
            CreatedBy = i.CreatedBy
        }));

        transfers.AddRange(recentExports.Select(e => new RecentTransferDto
        {
            Id = e.Id,
            Name = e.ExportName,
            Direction = TransferDirection.Export,
            Type = e.ExportType.ToString(),
            Status = e.Status.ToString(),
            RecordCount = e.TotalRecords,
            CreatedAt = e.CreatedAt,
            CreatedBy = e.CreatedBy
        }));

        return transfers
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToList();
    }

    public Task<Dictionary<string, long>> GetTransferVolumeTrendAsync(
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, aggregate by day
        var trend = new Dictionary<string, long>();
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        for (int i = 0; i <= days; i++)
        {
            var date = startDate.AddDays(i);
            trend[date.ToString("yyyy-MM-dd")] = 0;
        }

        return Task.FromResult(trend);
    }
}
