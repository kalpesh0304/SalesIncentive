-- =============================================
-- DORISE Sales Incentive Framework
-- Database Schema - Initial Migration
-- Version: V001
-- Created: January 2025
-- Author: Claude Code
-- =============================================

-- "I'm learnding!" - Ralph Wiggum
-- This schema captures everything we've learned about incentive management.

-- =============================================
-- SECTION 1: ORGANIZATIONAL STRUCTURE
-- =============================================

-- Company table
CREATE TABLE Company (
    CompanyId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    CompanyCode NVARCHAR(20) NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Company PRIMARY KEY (CompanyId),
    CONSTRAINT UQ_Company_Code UNIQUE (CompanyCode)
);

-- Zone table
CREATE TABLE Zone (
    ZoneId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    ZoneCode NVARCHAR(20) NOT NULL,
    ZoneName NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Zone PRIMARY KEY (ZoneId),
    CONSTRAINT FK_Zone_Company FOREIGN KEY (CompanyId) REFERENCES Company(CompanyId),
    CONSTRAINT UQ_Zone_Code UNIQUE (CompanyId, ZoneCode)
);

-- Store table
CREATE TABLE Store (
    StoreId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    ZoneId UNIQUEIDENTIFIER NOT NULL,
    StoreCode NVARCHAR(20) NOT NULL,
    StoreName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Store PRIMARY KEY (StoreId),
    CONSTRAINT FK_Store_Zone FOREIGN KEY (ZoneId) REFERENCES Zone(ZoneId),
    CONSTRAINT UQ_Store_Code UNIQUE (ZoneId, StoreCode)
);

-- Role table
CREATE TABLE Role (
    RoleId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    RoleCode NVARCHAR(20) NOT NULL,
    RoleName NVARCHAR(100) NOT NULL,
    RoleLevel INT NOT NULL DEFAULT 1,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_Role PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Role_Code UNIQUE (RoleCode)
);

-- =============================================
-- SECTION 2: EMPLOYEE (WITH TEMPORAL TABLE)
-- =============================================

-- Employee table with system versioning
CREATE TABLE Employee (
    EmployeeId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    EmployeeNumber NVARCHAR(20) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
    HireDate DATE NOT NULL,
    TerminationDate DATE NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    -- Temporal columns
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),

    CONSTRAINT PK_Employee PRIMARY KEY (EmployeeId),
    CONSTRAINT UQ_Employee_Number UNIQUE (EmployeeNumber),
    CONSTRAINT CK_Employee_Status CHECK (Status IN ('Active', 'Inactive', 'Terminated'))
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.EmployeeHistory));

-- Assignment table with system versioning
CREATE TABLE Assignment (
    AssignmentId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    EmployeeId UNIQUEIDENTIFIER NOT NULL,
    StoreId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    EffectiveFrom DATETIME2 NOT NULL,
    EffectiveTo DATETIME2 NULL,
    IsPrimary BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    -- Temporal columns
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo),

    CONSTRAINT PK_Assignment PRIMARY KEY (AssignmentId),
    CONSTRAINT FK_Assignment_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId),
    CONSTRAINT FK_Assignment_Store FOREIGN KEY (StoreId) REFERENCES Store(StoreId),
    CONSTRAINT FK_Assignment_Role FOREIGN KEY (RoleId) REFERENCES Role(RoleId)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.AssignmentHistory));

-- Split Share table
CREATE TABLE SplitShare (
    SplitShareId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    EmployeeId UNIQUEIDENTIFIER NOT NULL,
    SharePercentage DECIMAL(5,2) NOT NULL,
    ShareRole NVARCHAR(50) NOT NULL DEFAULT 'Primary',
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_SplitShare PRIMARY KEY (SplitShareId),
    CONSTRAINT FK_SplitShare_Assignment FOREIGN KEY (AssignmentId) REFERENCES Assignment(AssignmentId),
    CONSTRAINT FK_SplitShare_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId),
    CONSTRAINT CK_SplitShare_Percentage CHECK (SharePercentage > 0 AND SharePercentage <= 100)
);

-- =============================================
-- SECTION 3: INCENTIVE PLAN CONFIGURATION
-- =============================================

-- Incentive Plan table
CREATE TABLE IncentivePlan (
    PlanId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PlanCode NVARCHAR(20) NOT NULL,
    PlanName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_IncentivePlan PRIMARY KEY (PlanId),
    CONSTRAINT UQ_IncentivePlan_Code UNIQUE (PlanCode)
);

-- Plan Version table
CREATE TABLE PlanVersion (
    PlanVersionId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PlanId UNIQUEIDENTIFIER NOT NULL,
    VersionNumber INT NOT NULL,
    EffectiveFrom DATE NOT NULL,
    EffectiveTo DATE NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
    ApprovedBy NVARCHAR(256) NULL,
    ApprovedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_PlanVersion PRIMARY KEY (PlanVersionId),
    CONSTRAINT FK_PlanVersion_Plan FOREIGN KEY (PlanId) REFERENCES IncentivePlan(PlanId),
    CONSTRAINT UQ_PlanVersion UNIQUE (PlanId, VersionNumber),
    CONSTRAINT CK_PlanVersion_Status CHECK (Status IN ('Draft', 'Active', 'Superseded', 'Archived'))
);

-- Plan Slab table
CREATE TABLE PlanSlab (
    SlabId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PlanVersionId UNIQUEIDENTIFIER NOT NULL,
    SlabOrder INT NOT NULL,
    SlabName NVARCHAR(50) NOT NULL,
    MinThreshold DECIMAL(18,2) NOT NULL,
    MaxThreshold DECIMAL(18,2) NULL,
    RateType NVARCHAR(20) NOT NULL,
    RateValue DECIMAL(18,4) NOT NULL,
    CapAmount DECIMAL(18,2) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedBy NVARCHAR(256) NULL,

    CONSTRAINT PK_PlanSlab PRIMARY KEY (SlabId),
    CONSTRAINT FK_PlanSlab_Version FOREIGN KEY (PlanVersionId) REFERENCES PlanVersion(PlanVersionId),
    CONSTRAINT UQ_PlanSlab_Order UNIQUE (PlanVersionId, SlabOrder),
    CONSTRAINT CK_PlanSlab_RateType CHECK (RateType IN ('Percentage', 'Fixed', 'PerUnit', 'Tiered'))
);

-- Plan Role Mapping table
CREATE TABLE PlanRoleMapping (
    MappingId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PlanVersionId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_PlanRoleMapping PRIMARY KEY (MappingId),
    CONSTRAINT FK_PlanRoleMapping_Version FOREIGN KEY (PlanVersionId) REFERENCES PlanVersion(PlanVersionId),
    CONSTRAINT FK_PlanRoleMapping_Role FOREIGN KEY (RoleId) REFERENCES Role(RoleId),
    CONSTRAINT UQ_PlanRoleMapping UNIQUE (PlanVersionId, RoleId)
);

-- =============================================
-- SECTION 4: CALCULATION RECORDS (IMMUTABLE)
-- =============================================

-- Calculation Batch table
CREATE TABLE CalculationBatch (
    BatchId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Period NVARCHAR(7) NOT NULL,
    BatchType NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Running',
    StartedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedAt DATETIME2 NULL,
    TotalCount INT NOT NULL DEFAULT 0,
    SuccessCount INT NOT NULL DEFAULT 0,
    FailureCount INT NOT NULL DEFAULT 0,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedBy NVARCHAR(256) NOT NULL,

    CONSTRAINT PK_CalculationBatch PRIMARY KEY (BatchId),
    CONSTRAINT CK_CalculationBatch_Type CHECK (BatchType IN ('Daily', 'Weekly', 'Monthly', 'Manual')),
    CONSTRAINT CK_CalculationBatch_Status CHECK (Status IN ('Running', 'Completed', 'Failed', 'Cancelled'))
);

-- Calculation table (immutable - no updates allowed)
CREATE TABLE Calculation (
    CalculationId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    BatchId UNIQUEIDENTIFIER NOT NULL,
    EmployeeId UNIQUEIDENTIFIER NOT NULL,
    PlanVersionId UNIQUEIDENTIFIER NOT NULL,
    AssignmentId UNIQUEIDENTIFIER NOT NULL,
    Period NVARCHAR(7) NOT NULL,
    GrossAmount DECIMAL(18,2) NOT NULL,
    NetAmount DECIMAL(18,2) NOT NULL,
    ProrationFactor DECIMAL(5,4) NOT NULL DEFAULT 1.0000,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CalculatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    IsAmendment BIT NOT NULL DEFAULT 0,
    OriginalCalculationId UNIQUEIDENTIFIER NULL,
    AmendmentReason NVARCHAR(500) NULL,
    SupersededByCalculationId UNIQUEIDENTIFIER NULL,
    ExportedAt DATETIME2 NULL,
    ExportBatchId NVARCHAR(100) NULL,
    SnapshotJson NVARCHAR(MAX) NOT NULL,

    CONSTRAINT PK_Calculation PRIMARY KEY (CalculationId),
    CONSTRAINT FK_Calculation_Batch FOREIGN KEY (BatchId) REFERENCES CalculationBatch(BatchId),
    CONSTRAINT FK_Calculation_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId),
    CONSTRAINT FK_Calculation_PlanVersion FOREIGN KEY (PlanVersionId) REFERENCES PlanVersion(PlanVersionId),
    CONSTRAINT FK_Calculation_Assignment FOREIGN KEY (AssignmentId) REFERENCES Assignment(AssignmentId),
    CONSTRAINT FK_Calculation_Original FOREIGN KEY (OriginalCalculationId) REFERENCES Calculation(CalculationId),
    CONSTRAINT CK_Calculation_Status CHECK (Status IN ('Pending', 'Approved', 'Rejected', 'Exported', 'Superseded'))
);

-- Calculation Detail table
CREATE TABLE CalculationDetail (
    DetailId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    CalculationId UNIQUEIDENTIFIER NOT NULL,
    MetricType NVARCHAR(50) NOT NULL,
    MetricValue DECIMAL(18,2) NOT NULL,
    SlabApplied NVARCHAR(50) NOT NULL,
    RateType NVARCHAR(20) NOT NULL,
    RateApplied DECIMAL(18,4) NOT NULL,
    BaseAmount DECIMAL(18,2) NOT NULL,
    ProratedAmount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,

    CONSTRAINT PK_CalculationDetail PRIMARY KEY (DetailId),
    CONSTRAINT FK_CalculationDetail_Calculation FOREIGN KEY (CalculationId) REFERENCES Calculation(CalculationId)
);

-- Calculation Adjustment table
CREATE TABLE CalculationAdjustment (
    AdjustmentId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    CalculationId UNIQUEIDENTIFIER NOT NULL,
    AdjustmentType NVARCHAR(20) NOT NULL,
    AdjustmentAmount DECIMAL(18,2) NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_CalculationAdjustment PRIMARY KEY (AdjustmentId),
    CONSTRAINT FK_CalculationAdjustment_Calculation FOREIGN KEY (CalculationId) REFERENCES Calculation(CalculationId),
    CONSTRAINT CK_CalculationAdjustment_Type CHECK (AdjustmentType IN ('Cap', 'Minimum', 'Clawback', 'Proration', 'Other'))
);

-- =============================================
-- SECTION 5: APPROVAL WORKFLOW
-- =============================================

-- Approval table
CREATE TABLE Approval (
    ApprovalId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    CalculationId UNIQUEIDENTIFIER NOT NULL,
    ApprovalLevel INT NOT NULL DEFAULT 1,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    AssignedTo NVARCHAR(256) NULL,
    ApprovedBy NVARCHAR(256) NULL,
    ApprovedAt DATETIME2 NULL,
    Comments NVARCHAR(1000) NULL,
    DelegatedFrom NVARCHAR(256) NULL,
    DueDate DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Approval PRIMARY KEY (ApprovalId),
    CONSTRAINT FK_Approval_Calculation FOREIGN KEY (CalculationId) REFERENCES Calculation(CalculationId),
    CONSTRAINT CK_Approval_Status CHECK (Status IN ('Pending', 'Approved', 'Rejected', 'Delegated', 'Escalated'))
);

-- =============================================
-- SECTION 6: AUDIT AND LOGGING
-- =============================================

-- Audit Log table (append-only)
CREATE TABLE AuditLog (
    AuditLogId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UserId NVARCHAR(256) NOT NULL,
    UserName NVARCHAR(200) NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(100) NOT NULL,
    EntityId NVARCHAR(100) NOT NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IPAddress NVARCHAR(50) NULL,
    UserAgent NVARCHAR(500) NULL,
    CorrelationId UNIQUEIDENTIFIER NULL,
    AdditionalInfo NVARCHAR(MAX) NULL,

    CONSTRAINT PK_AuditLog PRIMARY KEY (AuditLogId)
);

-- System Log table
CREATE TABLE SystemLog (
    LogId BIGINT IDENTITY(1,1) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Level NVARCHAR(20) NOT NULL,
    Source NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    CorrelationId UNIQUEIDENTIFIER NULL,
    Properties NVARCHAR(MAX) NULL,

    CONSTRAINT PK_SystemLog PRIMARY KEY (LogId)
);

-- =============================================
-- SECTION 7: INTEGRATION TABLES
-- =============================================

-- Sales Data Staging table
CREATE TABLE StagingSalesData (
    StagingId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    BatchId UNIQUEIDENTIFIER NOT NULL,
    TransactionId NVARCHAR(100) NOT NULL,
    TransactionDate DATETIME2 NOT NULL,
    EmployeeNumber NVARCHAR(20) NOT NULL,
    StoreCode NVARCHAR(20) NOT NULL,
    SalesAmount DECIMAL(18,2) NOT NULL,
    UnitsSold INT NOT NULL DEFAULT 0,
    ProductCategory NVARCHAR(100) NULL,
    ProductSku NVARCHAR(100) NULL,
    RawJson NVARCHAR(MAX) NULL,
    ProcessedAt DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ErrorMessage NVARCHAR(500) NULL,
    ImportedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_StagingSalesData PRIMARY KEY (StagingId),
    CONSTRAINT UQ_StagingSalesData_Transaction UNIQUE (BatchId, TransactionId)
);

-- Sales Metrics table (aggregated)
CREATE TABLE SalesMetrics (
    MetricId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    EmployeeId UNIQUEIDENTIFIER NOT NULL,
    Period NVARCHAR(7) NOT NULL,
    SalesAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    UnitsSold INT NOT NULL DEFAULT 0,
    ProductionValue DECIMAL(18,2) NOT NULL DEFAULT 0,
    SellThroughRate DECIMAL(5,4) NOT NULL DEFAULT 0,
    TransactionCount INT NOT NULL DEFAULT 0,
    DataAsOf DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_SalesMetrics PRIMARY KEY (MetricId),
    CONSTRAINT FK_SalesMetrics_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId),
    CONSTRAINT UQ_SalesMetrics UNIQUE (EmployeeId, Period)
);

-- Export Log table
CREATE TABLE ExportLog (
    ExportId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Period NVARCHAR(7) NOT NULL,
    ExportType NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    RecordCount INT NOT NULL DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    FileName NVARCHAR(500) NULL,
    ExternalBatchId NVARCHAR(100) NULL,
    StartedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedAt DATETIME2 NULL,
    ExportedBy NVARCHAR(256) NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,

    CONSTRAINT PK_ExportLog PRIMARY KEY (ExportId),
    CONSTRAINT CK_ExportLog_Type CHECK (ExportType IN ('Payroll', 'Report', 'Archive')),
    CONSTRAINT CK_ExportLog_Status CHECK (Status IN ('Pending', 'InProgress', 'Completed', 'Failed'))
);

-- Webhook Delivery Log table
CREATE TABLE WebhookDeliveryLog (
    DeliveryId NVARCHAR(100) NOT NULL,
    EventType NVARCHAR(100) NOT NULL,
    Source NVARCHAR(100) NOT NULL,
    ReceivedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ProcessedAt DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Received',
    Payload NVARCHAR(MAX) NOT NULL,
    ErrorMessage NVARCHAR(500) NULL,

    CONSTRAINT PK_WebhookDeliveryLog PRIMARY KEY (DeliveryId),
    CONSTRAINT CK_WebhookDeliveryLog_Status CHECK (Status IN ('Received', 'Processed', 'Failed', 'Duplicate'))
);

-- =============================================
-- SECTION 8: INDEXES
-- =============================================

-- Employee indexes
CREATE NONCLUSTERED INDEX IX_Employee_Status ON Employee(Status) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Employee_Email ON Employee(Email);

-- Assignment indexes
CREATE NONCLUSTERED INDEX IX_Assignment_Employee ON Assignment(EmployeeId) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Assignment_Store ON Assignment(StoreId) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Assignment_Effective ON Assignment(EffectiveFrom, EffectiveTo) WHERE IsDeleted = 0;

-- Calculation indexes
CREATE NONCLUSTERED INDEX IX_Calculation_Period_Status ON Calculation(Period, Status)
    INCLUDE (EmployeeId, GrossAmount, NetAmount);
CREATE NONCLUSTERED INDEX IX_Calculation_Employee_Period ON Calculation(EmployeeId, Period);
CREATE NONCLUSTERED INDEX IX_Calculation_Batch ON Calculation(BatchId);

-- Approval indexes
CREATE NONCLUSTERED INDEX IX_Approval_Status_Level ON Approval(Status, ApprovalLevel)
    INCLUDE (CalculationId, AssignedTo);
CREATE NONCLUSTERED INDEX IX_Approval_AssignedTo ON Approval(AssignedTo) WHERE Status = 'Pending';

-- Audit indexes
CREATE NONCLUSTERED INDEX IX_AuditLog_Entity ON AuditLog(EntityType, EntityId, Timestamp DESC);
CREATE NONCLUSTERED INDEX IX_AuditLog_User ON AuditLog(UserId, Timestamp DESC);
CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp ON AuditLog(Timestamp DESC);

-- System Log indexes
CREATE NONCLUSTERED INDEX IX_SystemLog_Timestamp ON SystemLog(Timestamp DESC);
CREATE NONCLUSTERED INDEX IX_SystemLog_Level ON SystemLog(Level, Timestamp DESC);

-- Sales Metrics indexes
CREATE NONCLUSTERED INDEX IX_SalesMetrics_Period ON SalesMetrics(Period);

-- =============================================
-- SECTION 9: DEFAULT DATA
-- =============================================

-- Insert default roles
INSERT INTO Role (RoleId, RoleCode, RoleName, RoleLevel, Description, CreatedBy)
VALUES
    (NEWID(), 'SALES_REP', 'Sales Representative', 1, 'Front-line sales personnel', 'SYSTEM'),
    (NEWID(), 'SALES_MGR', 'Sales Manager', 2, 'Store or team sales manager', 'SYSTEM'),
    (NEWID(), 'ZONE_MGR', 'Zone Manager', 3, 'Regional zone manager', 'SYSTEM'),
    (NEWID(), 'DIRECTOR', 'Sales Director', 4, 'Sales director', 'SYSTEM');

-- Insert default company
INSERT INTO Company (CompanyId, CompanyCode, CompanyName, CreatedBy)
VALUES (NEWID(), 'DORISE', 'Dorise Corporation', 'SYSTEM');

GO

-- =============================================
-- SECTION 10: STORED PROCEDURES
-- =============================================

-- Get employee as of a specific date
CREATE OR ALTER PROCEDURE sp_GetEmployeeAsOf
    @EmployeeId UNIQUEIDENTIFIER,
    @AsOfDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Employee
    FOR SYSTEM_TIME AS OF @AsOfDate
    WHERE EmployeeId = @EmployeeId;
END;
GO

-- Get active employees for calculation period
CREATE OR ALTER PROCEDURE sp_GetActiveEmployeesForPeriod
    @Period NVARCHAR(7)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PeriodStart DATE = CAST(@Period + '-01' AS DATE);
    DECLARE @PeriodEnd DATE = EOMONTH(@PeriodStart);

    SELECT DISTINCT e.*
    FROM Employee e
    INNER JOIN Assignment a ON e.EmployeeId = a.EmployeeId
    WHERE e.Status = 'Active'
      AND e.IsDeleted = 0
      AND a.IsDeleted = 0
      AND e.HireDate <= @PeriodEnd
      AND (e.TerminationDate IS NULL OR e.TerminationDate >= @PeriodStart)
      AND a.EffectiveFrom <= @PeriodEnd
      AND (a.EffectiveTo IS NULL OR a.EffectiveTo >= @PeriodStart);
END;
GO

-- Process staging data to sales metrics
CREATE OR ALTER PROCEDURE sp_ProcessStagingToMetrics
    @BatchId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Aggregate staging data by employee and period
        MERGE SalesMetrics AS target
        USING (
            SELECT
                e.EmployeeId,
                FORMAT(s.TransactionDate, 'yyyy-MM') AS Period,
                SUM(s.SalesAmount) AS SalesAmount,
                SUM(s.UnitsSold) AS UnitsSold,
                COUNT(*) AS TransactionCount,
                MAX(s.TransactionDate) AS DataAsOf
            FROM StagingSalesData s
            INNER JOIN Employee e ON s.EmployeeNumber = e.EmployeeNumber
            WHERE s.BatchId = @BatchId
              AND s.Status = 'Pending'
            GROUP BY e.EmployeeId, FORMAT(s.TransactionDate, 'yyyy-MM')
        ) AS source
        ON target.EmployeeId = source.EmployeeId AND target.Period = source.Period
        WHEN MATCHED THEN
            UPDATE SET
                SalesAmount = target.SalesAmount + source.SalesAmount,
                UnitsSold = target.UnitsSold + source.UnitsSold,
                TransactionCount = target.TransactionCount + source.TransactionCount,
                DataAsOf = source.DataAsOf,
                ModifiedAt = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT (EmployeeId, Period, SalesAmount, UnitsSold, TransactionCount, DataAsOf)
            VALUES (source.EmployeeId, source.Period, source.SalesAmount, source.UnitsSold, source.TransactionCount, source.DataAsOf);

        -- Mark staging records as processed
        UPDATE StagingSalesData
        SET Status = 'Processed',
            ProcessedAt = SYSUTCDATETIME()
        WHERE BatchId = @BatchId
          AND Status = 'Pending';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- "Me fail English? That's unpossible!" - Ralph Wiggum
-- Database failures are also unpossible with proper constraints!

PRINT 'DSIF Initial Schema created successfully.';
GO
