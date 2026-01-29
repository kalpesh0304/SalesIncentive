using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Integrations.Services;
using Dorise.Incentive.Application.Notifications.Services;
using Dorise.Incentive.Application.Reports.Services;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Infrastructure.Integrations;
using Dorise.Incentive.Infrastructure.Notifications;
using Dorise.Incentive.Infrastructure.Persistence;
using Dorise.Incentive.Infrastructure.Persistence.Repositories;
using Dorise.Incentive.Infrastructure.Services;
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
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<IncentiveDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IncentiveDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

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

        return services;
    }
}
