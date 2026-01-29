using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for IncentivePlan.
/// "Sleep! That's where I'm a Viking!" - And plans dream big!
/// </summary>
public class IncentivePlanConfiguration : IEntityTypeConfiguration<IncentivePlan>
{
    public void Configure(EntityTypeBuilder<IncentivePlan> builder)
    {
        builder.ToTable("IncentivePlans", "incentive");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("IX_IncentivePlans_Code");

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.PlanType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Frequency)
            .HasConversion<int>()
            .IsRequired();

        // Value Object: DateRange (EffectivePeriod)
        builder.OwnsOne(p => p.EffectivePeriod, period =>
        {
            period.Property(d => d.StartDate)
                .HasColumnName("EffectiveFrom")
                .HasColumnType("date")
                .IsRequired();

            period.Property(d => d.EndDate)
                .HasColumnName("EffectiveTo")
                .HasColumnType("date")
                .IsRequired();
        });

        // Value Object: Target
        builder.OwnsOne(p => p.Target, target =>
        {
            target.Property(t => t.TargetValue)
                .HasColumnName("TargetValue")
                .HasPrecision(18, 2)
                .IsRequired();

            target.Property(t => t.MinimumThreshold)
                .HasColumnName("MinimumThreshold")
                .HasPrecision(18, 2)
                .IsRequired();

            target.Property(t => t.AchievementType)
                .HasColumnName("AchievementType")
                .HasConversion<int>()
                .IsRequired();

            target.Property(t => t.MetricUnit)
                .HasColumnName("MetricUnit")
                .HasMaxLength(50);
        });

        // Value Object: Money (MaximumPayout)
        builder.OwnsOne(p => p.MaximumPayout, payout =>
        {
            payout.Property(m => m.Amount)
                .HasColumnName("MaximumPayout")
                .HasPrecision(18, 2);

            payout.Property(m => m.Currency)
                .HasColumnName("MaxPayoutCurrency")
                .HasMaxLength(3);
        });

        // Value Object: Money (MinimumPayout)
        builder.OwnsOne(p => p.MinimumPayout, payout =>
        {
            payout.Property(m => m.Amount)
                .HasColumnName("MinimumPayout")
                .HasPrecision(18, 2);

            payout.Property(m => m.Currency)
                .HasColumnName("MinPayoutCurrency")
                .HasMaxLength(3);
        });

        builder.Property(p => p.RequiresApproval)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.ApprovalLevels)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(p => p.EligibilityCriteria)
            .HasMaxLength(2000);

        builder.Property(p => p.Version)
            .IsRequired()
            .HasDefaultValue(1);

        // Relationships
        builder.HasMany(p => p.Slabs)
            .WithOne()
            .HasForeignKey(s => s.IncentivePlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Assignments)
            .WithOne(a => a.IncentivePlan)
            .HasForeignKey(a => a.IncentivePlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_IncentivePlans_Status");

        builder.HasIndex(p => p.PlanType)
            .HasDatabaseName("IX_IncentivePlans_PlanType");

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

        builder.Ignore(p => p.DomainEvents);
    }
}
