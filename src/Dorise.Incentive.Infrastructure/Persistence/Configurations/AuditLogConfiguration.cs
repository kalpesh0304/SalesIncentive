using Dorise.Incentive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for AuditLog.
/// "What's a battle?" - It's a permanent record of all battles... I mean changes!
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLog");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AuditLogId");

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ChangedProperties)
            .HasMaxLength(4000);

        builder.Property(a => a.UserId);

        builder.Property(a => a.UserName)
            .HasMaxLength(200);

        builder.Property(a => a.UserEmail)
            .HasMaxLength(256);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.UserAgent)
            .HasMaxLength(1000);

        builder.Property(a => a.Reason)
            .HasMaxLength(1000);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.AdditionalData)
            .HasColumnType("nvarchar(max)");

        // Indexes for common queries
        builder.HasIndex(a => a.EntityType)
            .HasDatabaseName("IX_AuditLog_EntityType");

        builder.HasIndex(a => a.EntityId)
            .HasDatabaseName("IX_AuditLog_EntityId");

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_AuditLog_EntityType_EntityId");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_AuditLog_UserId");

        builder.HasIndex(a => a.Action)
            .HasDatabaseName("IX_AuditLog_Action");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_AuditLog_Timestamp");

        builder.HasIndex(a => a.CorrelationId)
            .HasDatabaseName("IX_AuditLog_CorrelationId");

        // Composite index for common searches
        builder.HasIndex(a => new { a.Timestamp, a.EntityType, a.Action })
            .HasDatabaseName("IX_AuditLog_Timestamp_EntityType_Action");
    }
}
