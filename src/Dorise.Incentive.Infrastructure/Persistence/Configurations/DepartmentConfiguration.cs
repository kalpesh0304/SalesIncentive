using Dorise.Incentive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Department.
/// "I dress myself!" - And this configures itself!
/// </summary>
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments", "incentive");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(d => d.Code)
            .IsUnique()
            .HasDatabaseName("IX_Departments_Code");

        builder.Property(d => d.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.CostCenter)
            .HasMaxLength(50);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.HierarchyLevel)
            .IsRequired()
            .HasDefaultValue(0);

        // Self-referencing relationship for hierarchy
        builder.HasOne<Department>()
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.ParentDepartmentId)
            .HasDatabaseName("IX_Departments_ParentDepartmentId");

        builder.HasIndex(d => d.IsActive)
            .HasDatabaseName("IX_Departments_IsActive");

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

        // Ignore navigation and domain events
        builder.Ignore(d => d.DomainEvents);
    }
}
