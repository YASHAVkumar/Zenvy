/* Production hardening for finance reporting, employee linkage, and roles. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Employees', 'UserId') IS NULL
    ALTER TABLE dbo.Employees ADD UserId nvarchar(150) NULL;

IF OBJECT_ID('dbo.ProfitDistributions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProfitDistributions
    (
        DistributionId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProfitDistributions PRIMARY KEY,
        InvestorId int NOT NULL,
        [Month] tinyint NOT NULL,
        [Year] smallint NOT NULL,
        ProfitAmount decimal(18,2) NOT NULL,
        DistributedDate datetime2 NULL,
        Notes nvarchar(500) NULL,
        CONSTRAINT FK_ProfitDistributions_Investor FOREIGN KEY (InvestorId) REFERENCES dbo.Investors(InvestorId)
    );
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Employees_UserId' AND object_id = OBJECT_ID('dbo.Employees'))
    EXEC(N'CREATE UNIQUE INDEX UX_Employees_UserId ON dbo.Employees(UserId) WHERE UserId IS NOT NULL');

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Employees_User')
    EXEC(N'ALTER TABLE dbo.Employees WITH CHECK ADD CONSTRAINT FK_Employees_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)');

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_ProfitDistributions_Period' AND object_id = OBJECT_ID('dbo.ProfitDistributions'))
    EXEC(N'CREATE UNIQUE INDEX UX_ProfitDistributions_Period ON dbo.ProfitDistributions([Year], [Month], InvestorId)');

UPDATE dbo.Users
SET Role = RTRIM(Role)
WHERE Role <> RTRIM(Role);

UPDATE dbo.Roles SET Name = 'Admin' WHERE RoleId = 1;
UPDATE dbo.Roles SET Name = 'Manager' WHERE RoleId = 2;
UPDATE dbo.Roles SET Name = 'InventoryManager' WHERE RoleId = 3;
UPDATE dbo.Roles SET Name = 'Accountant' WHERE RoleId = 4;
UPDATE dbo.Roles SET Name = 'SalesPerson' WHERE RoleId = 5;
UPDATE dbo.Roles SET Name = 'Investor' WHERE RoleId = 6;

COMMIT TRANSACTION;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeCompensationReport
    @FromDate datetime2,
    @ToDate datetime2,
    @EmployeeId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @FromDate > @ToDate THROW 50240, 'Invalid compensation period.', 1;
    DECLARE @EndDate datetime2 = DATEADD(DAY, 1, CAST(@ToDate AS date));

    SELECT e.EmployeeId, e.UserId, e.EmployeeName,
           CAST(ISNULL(s.BaseSalary, 0) AS decimal(18,2)) AS BaseSalary,
           CAST(ISNULL(c.CommissionAmount, 0) AS decimal(18,2)) AS CommissionAmount,
           ISNULL(c.CommissionCount, 0) AS CommissionCount,
           CAST(ISNULL(s.BaseSalary, 0) + ISNULL(c.CommissionAmount, 0) AS decimal(18,2)) AS TotalCompensation
    FROM dbo.Employees e
    OUTER APPLY (
        SELECT TOP (1) BaseSalary
        FROM dbo.Salary
        WHERE EmployeeId = e.EmployeeId AND IsActive = 1
        ORDER BY SalaryId DESC
    ) s
    OUTER APPLY (
        SELECT SUM(ec.CommissionAmount) AS CommissionAmount, COUNT_BIG(*) AS CommissionCount
        FROM dbo.EmployeeCommissions ec
        WHERE ec.UserId = e.UserId
          AND ec.CreatedAt >= @FromDate AND ec.CreatedAt < @EndDate
    ) c
    WHERE @EmployeeId IS NULL OR e.EmployeeId = @EmployeeId
    ORDER BY e.EmployeeName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DistributeInvestorProfit
    @Month tinyint, @Year smallint, @NetProfit decimal(18,2),
    @DistributedDate datetime2 = NULL, @Notes nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @Month NOT BETWEEN 1 AND 12 OR @Year < 2000 THROW 50223, 'Invalid distribution period.', 1;
    IF @NetProfit <= 0 THROW 50225, 'There is no positive net profit to distribute for this period.', 1;

    DECLARE @EndDate datetime2 = DATEADD(MONTH, 1, DATEFROMPARTS(@Year, @Month, 1));
    BEGIN TRANSACTION;
    IF EXISTS (SELECT 1 FROM dbo.ProfitDistributions WITH (UPDLOCK, HOLDLOCK) WHERE [Month] = @Month AND [Year] = @Year)
        THROW 50224, 'Profit has already been distributed for this period.', 1;

    INSERT INTO dbo.ProfitDistributions (InvestorId, [Month], [Year], ProfitAmount, DistributedDate, Notes)
    SELECT InvestorId, @Month, @Year, ROUND(@NetProfit * OwnershipPercent / 100.0, 2), @DistributedDate, @Notes
    FROM dbo.Investors WITH (UPDLOCK, HOLDLOCK)
    WHERE Status = 1 AND JoinDate < @EndDate;

    IF @@ROWCOUNT = 0 THROW 50226, 'No active investors were eligible for this period.', 1;
    COMMIT TRANSACTION;
END;
GO