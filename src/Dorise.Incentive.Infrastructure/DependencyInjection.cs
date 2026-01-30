using Dorise.Incentive.Application.Audit.Services;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Configuration.Services;
using Dorise.Incentive.Application.DataTransfer.Services;
using Dorise.Incentive.Application.Jobs.Services;
using Dorise.Incentive.Application.Performance.Services;
using Dorise.Incentive.Application.Dashboard.Services;
using Dorise.Incentive.Application.Integrations.Services;
using Dorise.Incentive.Application.Notifications.Services;
using Dorise.Incentive.Application.Reports.Services;
using Dorise.Incentive.Application.Security.Services;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Infrastructure.Audit;
using Dorise.Incentive.Infrastructure.Configuration;
using Dorise.Incentive.Infrastructure.Dashboard;
using Dorise.Incentive.Infrastructure.DataTransfer;
using Dorise.Incentive.Infrastructure.Integrations;
using Dorise.Incentive.Infrastructure.Jobs;
using Dorise.Incentive.Infrastructure.Caching;
using Dorise.Incentive.Infrastructure.Performance;
using Dorise.Incentive.Infrastructure.Notifications;
using Dorise.Incentive.Infrastructure.Persistence;
using Dorise.Incentive.Infrastructure.Persistence.Repositories;
using Dorise.Incentive.Infrastructure.Reports;
using Dorise.Incentive.Infrastructure.Security;
using Dorise.Incentive.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dorise.Incentive.Infrastructure;

/// <summary>
/// Dependency injection for Infrastructure layer.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there." - DI keeps everything connected properly!
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register audit interceptor first (needs HttpContextAccessor)
        services.AddHttpContextAccessor();
        services.AddScoped<AuditSaveChangesInterceptor>();

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<IncentiveDbContext>((serviceProvider, options) =>
        {
            var auditInterceptor = serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>();
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IncentiveDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            })
            .AddInterceptors(auditInterceptor);
        });

        // Repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IIncentivePlanRepository, IncentivePlanRepository>();
        services.AddScoped<ICalculationRepository, CalculationRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IApprovalRepository, ApprovalRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IExportService, ExportService>();

        // Integration Services
        services.AddScoped<IErpIntegrationService, ErpIntegrationService>();
        services.AddScoped<IHrIntegrationService, HrIntegrationService>();
        services.AddScoped<IPayrollIntegrationService, PayrollIntegrationService>();

        // Notification Services
        services.AddSingleton<ITemplateService, TemplateService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Audit Services
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();

        // Dashboard & Reporting Services
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportGenerationService, ReportGenerationService>();

        // Security & RBAC Services
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<ISecurityAuditService, SecurityAuditService>();

        // Authorization Handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ExtendedPermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();

        // Authorization Policy Provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Memory cache for permission caching
        services.AddMemoryCache();

        // Configuration Services
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
        services.AddScoped<ICalculationParameterRepository, CalculationParameterRepository>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<ICalculationParameterService, CalculationParameterService>();
        services.AddScoped<IConfigurationExportService, ConfigurationExportService>();

        // Job Services
        services.AddScoped<IBackgroundJobRepository, BackgroundJobRepository>();
        services.AddScoped<IJobScheduleRepository, JobScheduleRepository>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobScheduleService, JobScheduleService>();
        services.AddScoped<IBatchOperationService, BatchOperationService>();

        // Data Transfer Services
        services.AddScoped<IDataImportRepository, DataImportRepository>();
        services.AddScoped<IDataExportRepository, DataExportRepository>();
        services.AddScoped<IDataTransferTemplateRepository, DataTransferTemplateRepository>();
        services.AddScoped<IImportFieldMappingRepository, ImportFieldMappingRepository>();
        services.AddScoped<IDataImportService, DataImportService>();
        services.AddScoped<IDataExportService, DataExportService>();
        services.AddScoped<IDataTransferTemplateService, DataTransferTemplateService>();
        services.AddScoped<IFileParserService, FileParserService>();
        services.AddScoped<IDataTransferStatisticsService, DataTransferStatisticsService>();

        // Caching Services
        services.AddDistributedMemoryCache(); // Use Redis in production
        services.AddSingleton<ICacheService, DistributedCacheService>();
        services.AddSingleton<IQueryCacheService, QueryCacheService>();
        services.AddSingleton<IResponseCacheService, ResponseCacheService>();

        // Performance Services
        services.AddSingleton<IPerformanceMonitorService, PerformanceMonitorService>();
        services.AddSingleton<ICacheManagementService, CacheManagementService>();

        return services;
    }
}
