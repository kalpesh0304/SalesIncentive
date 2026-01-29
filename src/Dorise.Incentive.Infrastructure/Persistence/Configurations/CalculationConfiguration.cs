using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Calculation.
/// "What's a diorama?" - A calculation is better than a diorama!
/// </summary>
public class CalculationConfiguration : IEntityTypeConfiguration<Calculation>
{
    public void Configure(EntityTypeBuilder<Calculation> builder)
    {
        builder.ToTable("Calculations", "incentive");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        // Value Object: DateRange (CalculationPeriod)
        builder.OwnsOne(c => c.CalculationPeriod, period =>
        {
            period.Property(d => d.StartDate)
                .HasColumnName("PeriodStart")
                .HasColumnType("date")
                .IsRequired();

            period.Property(d => d.EndDate)
                .HasColumnName("PeriodEnd")
                .HasColumnType("date")
                .IsRequired();
        });

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.TargetValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.ActualValue)
            .HasPrecision(18, 2)
            .IsRequired();

        // Value Object: Percentage (AchievementPercentage)
        builder.Property(c => c.AchievementPercentage)
            .HasConversion(
                v => v.Value,
                v => Percentage.Create(v))
            .HasColumnName("AchievementPercentage")
            .HasPrecision(8, 4)
            .IsRequired();

        // Value Object: Money (BaseSalary)
        builder.OwnsOne(c => c.BaseSalary, salary =>
        {
            salary.Property(m => m.Amount)
                .HasColumnName("BaseSalary")
                .HasPrecision(18, 2)
                .IsRequired();

            salary.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object: Money (GrossIncentive)
        builder.OwnsOne(c => c.GrossIncentive, incentive =>
        {
            incentive.Property(m => m.Amount)
                .HasColumnName("GrossIncentive")
                .HasPrecision(18, 2)
                .IsRequired();

            incentive.Property(m => m.Currency)
                .HasColumnName("GrossCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object: Money (NetIncentive)
        builder.OwnsOne(c => c.NetIncentive, incentive =>
        {
            incentive.Property(m => m.Amount)
                .HasColumnName("NetIncentive")
                .HasPrecision(18, 2)
                .IsRequired();

            incentive.Property(m => m.Currency)
                .HasColumnName("NetCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object: Percentage (ProrataFactor) - nullable
        builder.Property(c => c.ProrataFactor)
            .HasConversion(
                v => v != null ? v.Value : (decimal?)null,
                v => v.HasValue ? Percentage.Create(v.Value) : null)
            .HasColumnName("ProrataFactor")
            .HasPrecision(8, 4);

        builder.Property(c => c.CalculatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(c => c.CalculatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.RejectionReason)
            .HasMaxLength(500);

        builder.Property(c => c.AdjustmentReason)
            .HasMaxLength(500);

        builder.Property(c => c.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        builder.HasOne(c => c.Employee)
            .WithMany(e => e.Calculations)
            .HasForeignKey(c => c.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.IncentivePlan)
            .WithMany()
            .HasForeignKey(c => c.IncentivePlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AppliedSlab)
            .WithMany()
            .HasForeignKey(c => c.AppliedSlabId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Approvals)
            .WithOne(a => a.Calculation)
            .HasForeignKey(a => a.CalculationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.EmployeeId)
            .HasDatabaseName("IX_Calculations_EmployeeId");

        builder.HasIndex(c => c.IncentivePlanId)
            .HasDatabaseName("IX_Calculations_IncentivePlanId");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("IX_Calculations_Status");

        builder.HasIndex(c => new { c.EmployeeId, c.IncentivePlanId })
            .HasDatabaseName("IX_Calculations_Employee_Plan");

        // Audit fields
        builder.Property("CreatedAt")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property("CreatedBy")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property("ModifiedAt")
            .HasColumnType("datetime2");

        builder.Property("ModifiedBy")
            .HasMaxLength(100);

        builder.Ignore(c => c.DomainEvents);
    }
}
