using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Employee.
/// "I'm a brick!" - Solid as a well-configured entity!
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees", "incentive");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        // Value Object: EmployeeCode
        builder.Property(e => e.EmployeeCode)
            .HasConversion(
                v => v.Value,
                v => EmployeeCode.Create(v))
            .HasColumnName("EmployeeCode")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique()
            .HasDatabaseName("IX_Employees_EmployeeCode");

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_Employees_Email");

        builder.Property(e => e.Designation)
            .HasMaxLength(100);

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.DateOfJoining)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.DateOfLeaving)
            .HasColumnType("date");

        // Value Object: Money (BaseSalary)
        builder.OwnsOne(e => e.BaseSalary, salary =>
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

        builder.Property(e => e.AzureAdObjectId)
            .HasMaxLength(50);

        builder.HasIndex(e => e.AzureAdObjectId)
            .HasDatabaseName("IX_Employees_AzureAdObjectId")
            .HasFilter("[AzureAdObjectId] IS NOT NULL");

        // Relationships
        builder.HasOne<Department>()
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DepartmentId)
            .HasDatabaseName("IX_Employees_DepartmentId");

        builder.HasIndex(e => e.ManagerId)
            .HasDatabaseName("IX_Employees_ManagerId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Employees_Status");

        // Audit fields
        ConfigureAuditFields(builder);

        // Ignore domain events collection
        builder.Ignore(e => e.DomainEvents);
    }

    private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> builder) where T : class
    {
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
    }
}
