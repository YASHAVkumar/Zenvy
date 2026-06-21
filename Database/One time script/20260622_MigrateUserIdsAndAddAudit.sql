USE Zenvy;
GO

SET XACT_ABORT ON;
GO

/* Run once on databases created with the old INT Users.UserId schema. */
IF EXISTS
(
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Users')
      AND name = 'UserId'
      AND system_type_id = TYPE_ID('int')
)
BEGIN
    BEGIN TRANSACTION;

    CREATE TABLE #UserIdMap (OldUserId INT PRIMARY KEY, NewUserId UNIQUEIDENTIFIER NOT NULL);
    INSERT INTO #UserIdMap (OldUserId, NewUserId) SELECT UserId, NEWID() FROM dbo.Users;

    ALTER TABLE dbo.Users ADD UserId_Guid UNIQUEIDENTIFIER NULL;
    UPDATE u SET UserId_Guid = m.NewUserId FROM dbo.Users u JOIN #UserIdMap m ON m.OldUserId = u.UserId;

    ALTER TABLE dbo.ProductMasters ADD CreatedBy_Guid UNIQUEIDENTIFIER NULL;
    UPDATE p SET CreatedBy_Guid = m.NewUserId FROM dbo.ProductMasters p JOIN #UserIdMap m ON m.OldUserId = TRY_CONVERT(INT, p.CreatedBy);
    ALTER TABLE dbo.InventoryTransactions ADD CreatedBy_Guid UNIQUEIDENTIFIER NULL;
    UPDATE i SET CreatedBy_Guid = m.NewUserId FROM dbo.InventoryTransactions i JOIN #UserIdMap m ON m.OldUserId = i.CreatedBy;
    ALTER TABLE dbo.PurchaseOrders ADD CreatedBy_Guid UNIQUEIDENTIFIER NULL;
    UPDATE p SET CreatedBy_Guid = m.NewUserId FROM dbo.PurchaseOrders p JOIN #UserIdMap m ON m.OldUserId = p.CreatedBy;
    ALTER TABLE dbo.SalesOrders ADD CreatedBy_Guid UNIQUEIDENTIFIER NULL;
    UPDATE s SET CreatedBy_Guid = m.NewUserId FROM dbo.SalesOrders s JOIN #UserIdMap m ON m.OldUserId = s.CreatedBy;
    ALTER TABLE dbo.SalesReturns ADD CreatedBy_Guid UNIQUEIDENTIFIER NULL;
    UPDATE r SET CreatedBy_Guid = m.NewUserId FROM dbo.SalesReturns r JOIN #UserIdMap m ON m.OldUserId = r.CreatedBy;
    ALTER TABLE dbo.Expenses ADD CreatedBy_Guid UNIQUEIDENTIFIER NULL;
    UPDATE e SET CreatedBy_Guid = m.NewUserId FROM dbo.Expenses e JOIN #UserIdMap m ON m.OldUserId = e.CreatedBy;
    ALTER TABLE dbo.EmployeeCommissions ADD UserId_Guid UNIQUEIDENTIFIER NULL;
    UPDATE e SET UserId_Guid = m.NewUserId FROM dbo.EmployeeCommissions e JOIN #UserIdMap m ON m.OldUserId = e.UserId;

    ALTER TABLE dbo.PurchaseOrders DROP CONSTRAINT FK_PO_User;
    ALTER TABLE dbo.SalesOrders DROP CONSTRAINT FK_SO_User;
    ALTER TABLE dbo.Expenses DROP CONSTRAINT FK_Expense_User;
    ALTER TABLE dbo.EmployeeCommissions DROP CONSTRAINT FK_Commission_User;

    DECLARE @PrimaryKey SYSNAME;
    SELECT @PrimaryKey = kc.name
    FROM sys.key_constraints kc
    WHERE kc.parent_object_id = OBJECT_ID('dbo.Users') AND kc.[type] = 'PK';
    EXEC(N'ALTER TABLE dbo.Users DROP CONSTRAINT ' + QUOTENAME(@PrimaryKey));

    ALTER TABLE dbo.Users DROP COLUMN UserId;
    ALTER TABLE dbo.ProductMasters DROP COLUMN CreatedBy;
    ALTER TABLE dbo.InventoryTransactions DROP COLUMN CreatedBy;
    ALTER TABLE dbo.PurchaseOrders DROP COLUMN CreatedBy;
    ALTER TABLE dbo.SalesOrders DROP COLUMN CreatedBy;
    ALTER TABLE dbo.SalesReturns DROP COLUMN CreatedBy;
    ALTER TABLE dbo.Expenses DROP COLUMN CreatedBy;
    ALTER TABLE dbo.EmployeeCommissions DROP COLUMN UserId;

    EXEC sys.sp_rename 'dbo.Users.UserId_Guid', 'UserId', 'COLUMN';
    EXEC sys.sp_rename 'dbo.ProductMasters.CreatedBy_Guid', 'CreatedBy', 'COLUMN';
    EXEC sys.sp_rename 'dbo.InventoryTransactions.CreatedBy_Guid', 'CreatedBy', 'COLUMN';
    EXEC sys.sp_rename 'dbo.PurchaseOrders.CreatedBy_Guid', 'CreatedBy', 'COLUMN';
    EXEC sys.sp_rename 'dbo.SalesOrders.CreatedBy_Guid', 'CreatedBy', 'COLUMN';
    EXEC sys.sp_rename 'dbo.SalesReturns.CreatedBy_Guid', 'CreatedBy', 'COLUMN';
    EXEC sys.sp_rename 'dbo.Expenses.CreatedBy_Guid', 'CreatedBy', 'COLUMN';
    EXEC sys.sp_rename 'dbo.EmployeeCommissions.UserId_Guid', 'UserId', 'COLUMN';

    ALTER TABLE dbo.Users ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.PurchaseOrders ALTER COLUMN CreatedBy UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.SalesOrders ALTER COLUMN CreatedBy UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.Expenses ALTER COLUMN CreatedBy UNIQUEIDENTIFIER NOT NULL;
    ALTER TABLE dbo.EmployeeCommissions ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;

    ALTER TABLE dbo.Users ADD CONSTRAINT PK_Users PRIMARY KEY(UserId);
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_UserId DEFAULT NEWSEQUENTIALID() FOR UserId;
    ALTER TABLE dbo.ProductMasters ADD CONSTRAINT FK_ProductMasters_User FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(UserId);
    ALTER TABLE dbo.InventoryTransactions ADD CONSTRAINT FK_InventoryTransactions_User FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(UserId);
    ALTER TABLE dbo.PurchaseOrders ADD CONSTRAINT FK_PO_User FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(UserId);
    ALTER TABLE dbo.SalesOrders ADD CONSTRAINT FK_SO_User FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(UserId);
    ALTER TABLE dbo.SalesReturns ADD CONSTRAINT FK_SalesReturns_User FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(UserId);
    ALTER TABLE dbo.Expenses ADD CONSTRAINT FK_Expense_User FOREIGN KEY(CreatedBy) REFERENCES dbo.Users(UserId);
    ALTER TABLE dbo.EmployeeCommissions ADD CONSTRAINT FK_Commission_User FOREIGN KEY(UserId) REFERENCES dbo.Users(UserId);

    COMMIT TRANSACTION;
END;
GO

IF OBJECT_ID('dbo.ApiAuditLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiAuditLogs
    (
        AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
        TraceId NVARCHAR(100) NOT NULL,
        UserId UNIQUEIDENTIFIER NULL,
        HttpMethod NVARCHAR(10) NOT NULL,
        Path NVARCHAR(2048) NOT NULL,
        QueryString NVARCHAR(2048) NULL,
        StatusCode INT NOT NULL,
        DurationMs BIGINT NOT NULL,
        IpAddress NVARCHAR(64) NULL,
        UserAgent NVARCHAR(512) NULL,
        ExceptionType NVARCHAR(500) NULL,
        ErrorMessage NVARCHAR(2048) NULL,
        ErrorDetails NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ApiAuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_ApiAuditLogs_User FOREIGN KEY(UserId) REFERENCES dbo.Users(UserId)
    );
    CREATE INDEX IX_ApiAuditLogs_CreatedAt ON dbo.ApiAuditLogs(CreatedAt DESC);
    CREATE INDEX IX_ApiAuditLogs_TraceId ON dbo.ApiAuditLogs(TraceId);
    CREATE INDEX IX_ApiAuditLogs_UserId_CreatedAt ON dbo.ApiAuditLogs(UserId, CreatedAt DESC);
END;
GO
