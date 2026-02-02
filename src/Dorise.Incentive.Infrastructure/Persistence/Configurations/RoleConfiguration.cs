using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Role entity.
/// "I'm pedaling backwards!" - Roles are the engine driving authorization forward!
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsSystem)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.Permissions)
            .IsRequired()
            .HasConversion<long>(); // Store as long for flags enum

        // Store extended permissions as comma-separated integers
        builder.Property<List<ExtendedPermission>>("_extendedPermissions")
            .HasColumnName("ExtendedPermissions")
            .HasConversion(
                (List<ExtendedPermission> v) => string.Join(',', v.Select(e => ((int)e).ToString())),
                (string v) => string.IsNullOrEmpty(v)
                    ? new List<ExtendedPermission>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => (ExtendedPermission)int.Parse(s))
                        .ToList())
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasMaxLength(256);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.HasIndex(r => r.IsActive)
            .HasDatabaseName("IX_Roles_IsActive");

        builder.HasIndex(r => r.IsSystem)
            .HasDatabaseName("IX_Roles_IsSystem");
    }
}

/// <summary>
/// EF Core configuration for UserRole entity.
/// "I ated the purple berries!" - User roles consumed for authorization checks!
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.UserId)
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .IsRequired();

        builder.Property(ur => ur.AssignedByUserId)
            .HasMaxLength(256);

        builder.Property(ur => ur.IsActive)
            .IsRequired();

        builder.Property(ur => ur.Scope)
            .HasMaxLength(500);

        // Audit fields
        builder.Property(ur => ur.CreatedAt)
            .IsRequired();

        builder.Property(ur => ur.CreatedBy)
            .HasMaxLength(256);

        builder.Property(ur => ur.ModifiedBy)
            .HasMaxLength(256);

        // Relationships
        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("IX_UserRoles_UserId");

        builder.HasIndex(ur => ur.RoleId)
            .HasDatabaseName("IX_UserRoles_RoleId");

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .HasDatabaseName("IX_UserRoles_UserId_RoleId");

        builder.HasIndex(ur => ur.IsActive)
            .HasDatabaseName("IX_UserRoles_IsActive");

        builder.HasIndex(ur => ur.ExpiresAt)
            .HasDatabaseName("IX_UserRoles_ExpiresAt");
    }
}
