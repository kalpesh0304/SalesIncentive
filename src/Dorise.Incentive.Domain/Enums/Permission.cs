namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Defines granular permissions for the incentive system.
/// "I'm a star! I'm a star!" - Every permission shines in its own way!
/// </summary>
[Flags]
public enum Permission
{
    None = 0,

    // Employee permissions
    EmployeeView = 1 << 0,
    EmployeeCreate = 1 << 1,
    EmployeeEdit = 1 << 2,
    EmployeeDelete = 1 << 3,
    EmployeeExport = 1 << 4,
    EmployeeImport = 1 << 5,

    // Department permissions
    DepartmentView = 1 << 6,
    DepartmentCreate = 1 << 7,
    DepartmentEdit = 1 << 8,
    DepartmentDelete = 1 << 9,

    // Incentive Plan permissions
    PlanView = 1 << 10,
    PlanCreate = 1 << 11,
    PlanEdit = 1 << 12,
    PlanDelete = 1 << 13,
    PlanActivate = 1 << 14,
    PlanDeactivate = 1 << 15,

    // Calculation permissions
    CalculationView = 1 << 16,
    CalculationRun = 1 << 17,
    CalculationRecalculate = 1 << 18,
    CalculationExport = 1 << 19,

    // Approval permissions
    ApprovalView = 1 << 20,
    ApprovalApprove = 1 << 21,
    ApprovalReject = 1 << 22,
    ApprovalBulkProcess = 1 << 23,

    // Report permissions
    ReportView = 1 << 24,
    ReportGenerate = 1 << 25,
    ReportExport = 1 << 26,
    ReportSchedule = 1 << 27,

    // Dashboard permissions
    DashboardView = 1 << 28,
    DashboardExport = 1 << 29,

    // Audit permissions
    AuditView = 1 << 30,
    AuditExport = 1 << 31,

    // Combined permission sets (must use long for these)
    // Note: Using individual permissions for admin to avoid overflow
}

/// <summary>
/// Extended permissions that don't fit in the flags enum.
/// "My cat's breath smells like cat food." - Extra permissions for extra needs!
/// </summary>
public enum ExtendedPermission
{
    // System Administration
    SystemConfig = 100,
    UserManage = 101,
    RoleManage = 102,
    SecurityManage = 103,

    // Integration permissions
    IntegrationErp = 200,
    IntegrationHr = 201,
    IntegrationPayroll = 202,
    IntegrationConfig = 203,

    // Notification permissions
    NotificationManage = 300,
    NotificationTemplateEdit = 301,

    // Data Management
    DataPurge = 400,
    DataBackup = 401,
    DataRestore = 402
}

/// <summary>
/// Permission categories for grouping.
/// </summary>
public enum PermissionCategory
{
    Employee,
    Department,
    IncentivePlan,
    Calculation,
    Approval,
    Report,
    Dashboard,
    Audit,
    System,
    Integration,
    Notification,
    DataManagement
}
