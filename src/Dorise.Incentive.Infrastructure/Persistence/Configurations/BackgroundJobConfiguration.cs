using Dorise.Incentive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for BackgroundJob.
/// "I'm Idaho!" - And I schedule jobs to run in Idaho... or anywhere!
/// </summary>
public class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
{
    public void Configure(EntityTypeBuilder<BackgroundJob> builder)
    {
        builder.ToTable("BackgroundJob");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasColumnName("BackgroundJobId");

        builder.Property(j => j.JobName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.JobType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(j => j.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(j => j.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(j => j.Parameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(j => j.Result)
            .HasColumnType("nvarchar(max)");

        builder.Property(j => j.ErrorMessage)
            .HasMaxLength(4000);

        builder.Property(j => j.StackTrace)
            .HasColumnType("nvarchar(max)");

        builder.Property(j => j.CorrelationId)
            .HasMaxLength(100);

        builder.Property(j => j.TriggerInfo)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(j => j.CreatedBy)
            .HasMaxLength(200);

        builder.Property(j => j.ModifiedBy)
            .HasMaxLength(200);

        // Indexes for common queries
        builder.HasIndex(j => j.JobName)
            .HasDatabaseName("IX_BackgroundJob_JobName");

        builder.HasIndex(j => j.JobType)
            .HasDatabaseName("IX_BackgroundJob_JobType");

        builder.HasIndex(j => j.Status)
            .HasDatabaseName("IX_BackgroundJob_Status");

        builder.HasIndex(j => j.Priority)
            .HasDatabaseName("IX_BackgroundJob_Priority");

        builder.HasIndex(j => j.ScheduledAt)
            .HasDatabaseName("IX_BackgroundJob_ScheduledAt");

        builder.HasIndex(j => j.CreatedAt)
            .HasDatabaseName("IX_BackgroundJob_CreatedAt");

        builder.HasIndex(j => j.CorrelationId)
            .HasDatabaseName("IX_BackgroundJob_CorrelationId");

        builder.HasIndex(j => j.ParentJobId)
            .HasDatabaseName("IX_BackgroundJob_ParentJobId");

        // Composite indexes for queue processing
        builder.HasIndex(j => new { j.Status, j.Priority, j.ScheduledAt })
            .HasDatabaseName("IX_BackgroundJob_Status_Priority_ScheduledAt");

        builder.HasIndex(j => new { j.Status, j.CreatedAt })
            .HasDatabaseName("IX_BackgroundJob_Status_CreatedAt");

        builder.HasIndex(j => new { j.JobType, j.Status })
            .HasDatabaseName("IX_BackgroundJob_JobType_Status");
    }
}

/// <summary>
/// Entity configuration for JobSchedule.
/// "Hi, Super Nintendo Chalmers!" - Schedules are super at running jobs on time!
/// </summary>
public class JobScheduleConfiguration : IEntityTypeConfiguration<JobSchedule>
{
    public void Configure(EntityTypeBuilder<JobSchedule> builder)
    {
        builder.ToTable("JobSchedule");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("JobScheduleId");

        builder.Property(s => s.ScheduleName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.JobName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.JobType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.Parameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.CronExpression)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.TimeZoneId)
            .HasMaxLength(100);

        // Audit fields
        builder.Property(s => s.CreatedBy)
            .HasMaxLength(200);

        builder.Property(s => s.ModifiedBy)
            .HasMaxLength(200);

        // Indexes
        builder.HasIndex(s => s.ScheduleName)
            .IsUnique()
            .HasDatabaseName("IX_JobSchedule_ScheduleName");

        builder.HasIndex(s => s.JobName)
            .HasDatabaseName("IX_JobSchedule_JobName");

        builder.HasIndex(s => s.IsEnabled)
            .HasDatabaseName("IX_JobSchedule_IsEnabled");

        builder.HasIndex(s => s.NextRunAt)
            .HasDatabaseName("IX_JobSchedule_NextRunAt");

        // Composite index for scheduler queries
        builder.HasIndex(s => new { s.IsEnabled, s.NextRunAt })
            .HasDatabaseName("IX_JobSchedule_IsEnabled_NextRunAt");
    }
}
