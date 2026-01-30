using Dorise.Incentive.Application.Notifications.DTOs;
using Dorise.Incentive.Application.Notifications.Services;
using Dorise.Incentive.Domain.Events;
using Dorise.Incentive.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Notifications.EventHandlers;

/// <summary>
/// Handles CalculationCompletedEvent to send notifications.
/// "Sleep! That's where I'm a Viking!" - Notifications! That's where we inform people!
/// </summary>
public class CalculationCompletedNotificationHandler : INotificationHandler<CalculationCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICalculationRepository _calculationRepository;
    private readonly IIncentivePlanRepository _planRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CalculationCompletedNotificationHandler> _logger;

    public CalculationCompletedNotificationHandler(
        INotificationService notificationService,
        IEmployeeRepository employeeRepository,
        ICalculationRepository calculationRepository,
        IIncentivePlanRepository planRepository,
        IConfiguration configuration,
        ILogger<CalculationCompletedNotificationHandler> logger)
    {
        _notificationService = notificationService;
        _employeeRepository = employeeRepository;
        _calculationRepository = calculationRepository;
        _planRepository = planRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(CalculationCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CalculationCompletedEvent for calculation {CalculationId}",
            notification.CalculationId);

        try
        {
            var calculation = await _calculationRepository.GetByIdAsync(
                notification.CalculationId, cancellationToken);

            if (calculation == null) return;

            var employee = await _employeeRepository.GetByIdAsync(
                notification.EmployeeId, cancellationToken);

            if (employee == null) return;

            var plan = await _planRepository.GetByIdAsync(
                calculation.IncentivePlanId, cancellationToken);

            var templateData = new Dictionary<string, object>
            {
                ["EmployeeName"] = employee.FullName,
                ["Period"] = calculation.CalculationPeriod.ToString(),
                ["PlanName"] = plan?.Name ?? "Unknown Plan",
                ["Achievement"] = calculation.AchievementPercentage.Value.ToString("F1"),
                ["Currency"] = calculation.NetIncentive.Currency,
                ["Amount"] = calculation.NetIncentive.Amount.ToString("N2")
            };

            await _notificationService.SendFromTemplateAsync(
                employee.Id,
                "calculation-completed",
                templateData,
                cancellationToken);

            _logger.LogInformation(
                "Calculation completed notification sent to {EmployeeId}",
                employee.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send calculation completed notification");
        }
    }
}

/// <summary>
/// Handles CalculationSubmittedForApprovalEvent to notify approvers.
/// </summary>
public class ApprovalRequestedNotificationHandler : INotificationHandler<CalculationSubmittedForApprovalEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICalculationRepository _calculationRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApprovalRequestedNotificationHandler> _logger;

    public ApprovalRequestedNotificationHandler(
        INotificationService notificationService,
        IEmployeeRepository employeeRepository,
        ICalculationRepository calculationRepository,
        IConfiguration configuration,
        ILogger<ApprovalRequestedNotificationHandler> logger)
    {
        _notificationService = notificationService;
        _employeeRepository = employeeRepository;
        _calculationRepository = calculationRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(CalculationSubmittedForApprovalEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CalculationSubmittedForApprovalEvent for calculation {CalculationId}",
            notification.CalculationId);

        try
        {
            var calculation = await _calculationRepository.GetWithDetailsAsync(
                notification.CalculationId, cancellationToken);

            if (calculation == null) return;

            var employee = await _employeeRepository.GetByIdAsync(
                notification.EmployeeId, cancellationToken);

            if (employee == null) return;

            // Get the manager/approver
            if (!employee.ManagerId.HasValue) return;

            var manager = await _employeeRepository.GetByIdAsync(
                employee.ManagerId.Value, cancellationToken);

            if (manager == null) return;

            var baseUrl = _configuration["Application:BaseUrl"] ?? "https://incentive.dorise.com";

            var templateData = new Dictionary<string, object>
            {
                ["ApproverName"] = manager.FullName,
                ["EmployeeName"] = employee.FullName,
                ["Period"] = calculation.CalculationPeriod.ToString(),
                ["Currency"] = calculation.NetIncentive.Currency,
                ["Amount"] = calculation.NetIncentive.Amount.ToString("N2"),
                ["ApprovalLevel"] = "L1 - Manager",
                ["ActionUrl"] = $"{baseUrl}/approvals/{notification.CalculationId}",
                ["SlaHours"] = "72"
            };

            await _notificationService.SendFromTemplateAsync(
                manager.Id,
                "approval-pending",
                templateData,
                cancellationToken);

            _logger.LogInformation(
                "Approval pending notification sent to {ManagerId}",
                manager.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval pending notification");
        }
    }
}

/// <summary>
/// Handles CalculationApprovedEvent to notify the employee.
/// </summary>
public class CalculationApprovedNotificationHandler : INotificationHandler<CalculationApprovedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICalculationRepository _calculationRepository;
    private readonly ILogger<CalculationApprovedNotificationHandler> _logger;

    public CalculationApprovedNotificationHandler(
        INotificationService notificationService,
        IEmployeeRepository employeeRepository,
        ICalculationRepository calculationRepository,
        ILogger<CalculationApprovedNotificationHandler> logger)
    {
        _notificationService = notificationService;
        _employeeRepository = employeeRepository;
        _calculationRepository = calculationRepository;
        _logger = logger;
    }

    public async Task Handle(CalculationApprovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CalculationApprovedEvent for calculation {CalculationId}",
            notification.CalculationId);

        try
        {
            var calculation = await _calculationRepository.GetByIdAsync(
                notification.CalculationId, cancellationToken);

            if (calculation == null) return;

            var employee = await _employeeRepository.GetByIdAsync(
                notification.EmployeeId, cancellationToken);

            if (employee == null) return;

            var templateData = new Dictionary<string, object>
            {
                ["EmployeeName"] = employee.FullName,
                ["Period"] = calculation.CalculationPeriod.ToString(),
                ["Currency"] = calculation.NetIncentive.Currency,
                ["Amount"] = calculation.NetIncentive.Amount.ToString("N2"),
                ["ApprovedBy"] = notification.ApprovedBy,
                ["ApprovedDate"] = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm")
            };

            await _notificationService.SendFromTemplateAsync(
                employee.Id,
                "approval-approved",
                templateData,
                cancellationToken);

            _logger.LogInformation(
                "Approval approved notification sent to {EmployeeId}",
                employee.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval approved notification");
        }
    }
}

/// <summary>
/// Handles CalculationRejectedEvent to notify the employee.
/// </summary>
public class CalculationRejectedNotificationHandler : INotificationHandler<CalculationRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICalculationRepository _calculationRepository;
    private readonly ILogger<CalculationRejectedNotificationHandler> _logger;

    public CalculationRejectedNotificationHandler(
        INotificationService notificationService,
        IEmployeeRepository employeeRepository,
        ICalculationRepository calculationRepository,
        ILogger<CalculationRejectedNotificationHandler> logger)
    {
        _notificationService = notificationService;
        _employeeRepository = employeeRepository;
        _calculationRepository = calculationRepository;
        _logger = logger;
    }

    public async Task Handle(CalculationRejectedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CalculationRejectedEvent for calculation {CalculationId}",
            notification.CalculationId);

        try
        {
            var calculation = await _calculationRepository.GetByIdAsync(
                notification.CalculationId, cancellationToken);

            if (calculation == null) return;

            var employee = await _employeeRepository.GetByIdAsync(
                notification.EmployeeId, cancellationToken);

            if (employee == null) return;

            var templateData = new Dictionary<string, object>
            {
                ["EmployeeName"] = employee.FullName,
                ["Period"] = calculation.CalculationPeriod.ToString(),
                ["RejectedBy"] = notification.RejectedBy,
                ["Reason"] = notification.Reason
            };

            await _notificationService.SendFromTemplateAsync(
                employee.Id,
                "approval-rejected",
                templateData,
                cancellationToken);

            _logger.LogInformation(
                "Approval rejected notification sent to {EmployeeId}",
                employee.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval rejected notification");
        }
    }
}

/// <summary>
/// Handles CalculationPaidEvent to notify the employee.
/// </summary>
public class CalculationPaidNotificationHandler : INotificationHandler<CalculationPaidEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICalculationRepository _calculationRepository;
    private readonly ILogger<CalculationPaidNotificationHandler> _logger;

    public CalculationPaidNotificationHandler(
        INotificationService notificationService,
        IEmployeeRepository employeeRepository,
        ICalculationRepository calculationRepository,
        ILogger<CalculationPaidNotificationHandler> logger)
    {
        _notificationService = notificationService;
        _employeeRepository = employeeRepository;
        _calculationRepository = calculationRepository;
        _logger = logger;
    }

    public async Task Handle(CalculationPaidEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CalculationPaidEvent for calculation {CalculationId}",
            notification.CalculationId);

        try
        {
            var calculation = await _calculationRepository.GetByIdAsync(
                notification.CalculationId, cancellationToken);

            if (calculation == null) return;

            var employee = await _employeeRepository.GetByIdAsync(
                notification.EmployeeId, cancellationToken);

            if (employee == null) return;

            var templateData = new Dictionary<string, object>
            {
                ["EmployeeName"] = employee.FullName,
                ["Period"] = calculation.CalculationPeriod.ToString(),
                ["Currency"] = calculation.NetIncentive.Currency,
                ["NetAmount"] = notification.Amount.ToString("N2"),
                ["PaymentDate"] = DateTime.UtcNow.ToString("dd MMM yyyy"),
                ["PaymentReference"] = $"PAY-{notification.CalculationId.ToString()[..8].ToUpper()}"
            };

            await _notificationService.SendFromTemplateAsync(
                employee.Id,
                "payment-processed",
                templateData,
                cancellationToken);

            _logger.LogInformation(
                "Payment processed notification sent to {EmployeeId}",
                employee.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send payment processed notification");
        }
    }
}

/// <summary>
/// Handles EmployeeCreatedEvent to send welcome notification.
/// </summary>
public class EmployeeWelcomeNotificationHandler : INotificationHandler<EmployeeCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmployeeWelcomeNotificationHandler> _logger;

    public EmployeeWelcomeNotificationHandler(
        INotificationService notificationService,
        IEmployeeRepository employeeRepository,
        IConfiguration configuration,
        ILogger<EmployeeWelcomeNotificationHandler> logger)
    {
        _notificationService = notificationService;
        _employeeRepository = employeeRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(EmployeeCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling EmployeeCreatedEvent for employee {EmployeeId}",
            notification.EmployeeId);

        try
        {
            var employee = await _employeeRepository.GetByIdAsync(
                notification.EmployeeId, cancellationToken);

            if (employee == null) return;

            var baseUrl = _configuration["Application:BaseUrl"] ?? "https://incentive.dorise.com";

            var templateData = new Dictionary<string, object>
            {
                ["EmployeeName"] = employee.FullName,
                ["LoginUrl"] = $"{baseUrl}/login"
            };

            await _notificationService.SendFromTemplateAsync(
                employee.Id,
                "welcome",
                templateData,
                cancellationToken);

            _logger.LogInformation(
                "Welcome notification sent to {EmployeeId}",
                employee.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome notification");
        }
    }
}
