using Dorise.Incentive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for DataImport.
/// "I choo-choo-choose you!" - Choosing the right configuration for imports!
/// </summary>
public class DataImportConfiguration : IEntityTypeConfiguration<DataImport>
{
    public void Configure(EntityTypeBuilder<DataImport> builder)
    {
        builder.ToTable("DataImport");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("DataImportId");

        builder.Property(i => i.ImportName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.ImportType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(i => i.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.OriginalFileName)
            .HasMaxLength(500);

        builder.Property(i => i.FileFormat)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.MappingConfiguration)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.ValidationRules)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.ErrorLog)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.WarningLog)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.Options)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.DryRunResults)
            .HasColumnType("nvarchar(max)");

        builder.Property(i => i.CreatedBy)
            .HasMaxLength(200);

        builder.Property(i => i.ModifiedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(i => i.ImportType)
            .HasDatabaseName("IX_DataImport_ImportType");

        builder.HasIndex(i => i.Status)
            .HasDatabaseName("IX_DataImport_Status");

        builder.HasIndex(i => i.CreatedAt)
            .HasDatabaseName("IX_DataImport_CreatedAt");

        builder.HasIndex(i => i.BackgroundJobId)
            .HasDatabaseName("IX_DataImport_BackgroundJobId");

        builder.HasIndex(i => new { i.ImportType, i.Status })
            .HasDatabaseName("IX_DataImport_ImportType_Status");

        builder.HasIndex(i => new { i.Status, i.CreatedAt })
            .HasDatabaseName("IX_DataImport_Status_CreatedAt");
    }
}

/// <summary>
/// Entity configuration for DataExport.
/// "Me fail English? That's unpossible!" - Export configuration is unpossibly important!
/// </summary>
public class DataExportConfiguration : IEntityTypeConfiguration<DataExport>
{
    public void Configure(EntityTypeBuilder<DataExport> builder)
    {
        builder.ToTable("DataExport");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("DataExportId");

        builder.Property(e => e.ExportName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ExportType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.FileFormat)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.FileName)
            .HasMaxLength(500);

        builder.Property(e => e.FilePath)
            .HasMaxLength(1000);

        builder.Property(e => e.DownloadUrl)
            .HasMaxLength(2000);

        builder.Property(e => e.FilterCriteria)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ColumnConfiguration)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Options)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(200);

        builder.Property(e => e.ModifiedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(e => e.ExportType)
            .HasDatabaseName("IX_DataExport_ExportType");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_DataExport_Status");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_DataExport_CreatedAt");

        builder.HasIndex(e => e.BackgroundJobId)
            .HasDatabaseName("IX_DataExport_BackgroundJobId");

        builder.HasIndex(e => e.DownloadUrlExpiry)
            .HasDatabaseName("IX_DataExport_DownloadUrlExpiry");

        builder.HasIndex(e => new { e.ExportType, e.Status })
            .HasDatabaseName("IX_DataExport_ExportType_Status");

        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_DataExport_Status_CreatedAt");
    }
}

/// <summary>
/// Entity configuration for DataTransferTemplate.
/// "When I grow up, I want to be a principal or a caterpillar." - Templates grow into transfers!
/// </summary>
public class DataTransferTemplateConfiguration : IEntityTypeConfiguration<DataTransferTemplate>
{
    public void Configure(EntityTypeBuilder<DataTransferTemplate> builder)
    {
        builder.ToTable("DataTransferTemplate");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("DataTransferTemplateId");

        builder.Property(t => t.TemplateName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Direction)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.FileFormat)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.MappingConfiguration)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.ValidationRules)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.ColumnConfiguration)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.DefaultOptions)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.CreatedBy)
            .HasMaxLength(200);

        builder.Property(t => t.ModifiedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(t => t.TemplateName)
            .IsUnique()
            .HasDatabaseName("IX_DataTransferTemplate_TemplateName");

        builder.HasIndex(t => t.Direction)
            .HasDatabaseName("IX_DataTransferTemplate_Direction");

        builder.HasIndex(t => t.EntityType)
            .HasDatabaseName("IX_DataTransferTemplate_EntityType");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_DataTransferTemplate_IsActive");

        builder.HasIndex(t => new { t.Direction, t.EntityType, t.IsDefault })
            .HasDatabaseName("IX_DataTransferTemplate_Direction_EntityType_IsDefault");
    }
}

/// <summary>
/// Entity configuration for ImportFieldMapping.
/// "I bent my Wookie!" - Bending fields into the right mapping!
/// </summary>
public class ImportFieldMappingConfiguration : IEntityTypeConfiguration<ImportFieldMapping>
{
    public void Configure(EntityTypeBuilder<ImportFieldMapping> builder)
    {
        builder.ToTable("ImportFieldMapping");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("ImportFieldMappingId");

        builder.Property(m => m.SourceField)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.TargetField)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.DataType)
            .HasMaxLength(50);

        builder.Property(m => m.DefaultValue)
            .HasMaxLength(500);

        builder.Property(m => m.TransformExpression)
            .HasMaxLength(1000);

        builder.Property(m => m.ValidationRegex)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(m => m.DataImportId)
            .HasDatabaseName("IX_ImportFieldMapping_DataImportId");
    }
}
