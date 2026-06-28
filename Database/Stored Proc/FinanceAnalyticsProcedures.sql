USE Zenvy;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateExpenseType
    @Name NVARCHAR(100),
    @Description NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET @Name = LTRIM(RTRIM(@Name));
    IF @Name = '' THROW 50200, 'Expense type name is required.', 1;
    IF EXISTS (SELECT 1 FROM ExpenseTypes WHERE Name = @Name)
        THROW 50201, 'An expense type with this name already exists.', 1;

    INSERT INTO ExpenseTypes (Name, Description) VALUES (@Name, @Description);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ExpenseTypeId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenseTypes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ExpenseTypeId, Name, Description FROM ExpenseTypes ORDER BY Name;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_CreateExpense
    @ExpenseTypeId INT,
    @PoId Int,
    @Amount DECIMAL(18,2),
    @Description NVARCHAR(500) = NULL,
    @ExpenseDate DATETIME2,
    @CreatedBy varchar(150)
AS
BEGIN
    SET NOCOUNT ON;
    IF @Amount <= 0 THROW 50202, 'Expense amount must be greater than zero.', 1;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @CreatedBy AND IsActive = 1)
        THROW 50204, 'Active creating user not found.', 1;
    IF NOT EXISTS (SELECT 1 FROM PurchaseOrders WHERE POId = @PoId)
        THROW 50204, 'Purchase order not found.', 1;
    INSERT INTO Expenses (ExpenseTypeId,POId, Amount, Description, ExpenseDate, CreatedBy)
    VALUES (@ExpenseTypeId,@PoId, @Amount, @Description, @ExpenseDate, @CreatedBy);
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS ExpenseId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetExpenses
    @FromDate DATETIME2 = NULL,
    @ToDate DATETIME2 = NULL,
    @ExpenseTypeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT e.ExpenseId, e.ExpenseTypeId, et.Name AS ExpenseTypeName, e.Amount,
           e.Description, e.ExpenseDate, e.CreatedBy, u.FullName AS CreatedByName
    FROM Expenses e
    INNER JOIN ExpenseTypes et ON et.ExpenseTypeId = e.ExpenseTypeId
    INNER JOIN Users u ON u.UserId = e.CreatedBy
    WHERE (@FromDate IS NULL OR e.ExpenseDate >= @FromDate)
      AND (@ToDate IS NULL OR e.ExpenseDate < DATEADD(DAY, 1, CAST(@ToDate AS DATE)))
      AND (@ExpenseTypeId IS NULL OR e.ExpenseTypeId = @ExpenseTypeId)
    ORDER BY e.ExpenseDate DESC, e.ExpenseId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetProfitSourceData
    @FromDate DATETIME2,
    @ToDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    IF @FromDate > @ToDate THROW 50203, 'Invalid profit period.', 1;
    DECLARE @EndDate DATETIME2 = DATEADD(DAY, 1, CAST(@ToDate AS DATE));

    -- Raw purchase investment by variant. Profit arithmetic is intentionally performed in C#.
    SELECT pol.VariantId, CAST(SUM(pol.Qty) AS INT) AS Quantity,
           CAST(SUM(pol.Qty * pol.UnitCost) / NULLIF(SUM(pol.Qty), 0) AS DECIMAL(18,4)) AS UnitCost
    FROM PurchaseOrderLines pol
    INNER JOIN PurchaseOrders po ON po.POId = pol.POId
    WHERE po.OrderDate < @EndDate AND UPPER(po.Status) <> 'CANCELLED'
    GROUP BY pol.VariantId;

    SELECT sol.VariantId, sol.Qty AS Quantity,
           CAST((sol.Qty * sol.UnitPrice) - sol.Discount + sol.Tax +
                ISNULL(so.ShippingFee * ((sol.Qty * sol.UnitPrice) / NULLIF(so.SubTotal, 0)), 0) AS DECIMAL(18,2)) AS Revenue
    FROM SalesOrderLines sol
    INNER JOIN SalesOrders so ON so.OrderId = sol.OrderId
    WHERE so.OrderDate >= @FromDate AND so.OrderDate < @EndDate AND UPPER(so.Status) <> 'CANCELLED';

    SELECT srl.VariantId, srl.Qty AS Quantity,
           CAST(CASE
                WHEN COL_LENGTH('SalesReturnLines', 'RefundAmount') IS NOT NULL THEN srl.RefundAmount
                ELSE (srl.Qty * sol.UnitPrice) - (sol.Discount * srl.Qty / NULLIF(sol.Qty, 0)) + (sol.Tax * srl.Qty / NULLIF(sol.Qty, 0))
           END AS DECIMAL(18,2)) AS RefundAmount
    FROM SalesReturns sr
    INNER JOIN SalesReturnLines srl ON srl.ReturnId = sr.ReturnId
    INNER JOIN SalesOrderLines sol ON sol.OrderLineId = srl.OrderLineId
    WHERE sr.ReturnDate >= @FromDate AND sr.ReturnDate < @EndDate AND UPPER(ISNULL(sr.Status, '')) <> 'REJECTED';

    SELECT Amount FROM Expenses WHERE ExpenseDate >= @FromDate AND ExpenseDate < @EndDate;
    SELECT CommissionAmount AS Amount FROM EmployeeCommissions WHERE CreatedAt >= @FromDate AND CreatedAt < @EndDate;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateEmployeeCommission
    @UserId varchar(150),
    @OrderId BIGINT,
    @CommissionPercent DECIMAL(5,2)
AS
BEGIN
    SET NOCOUNT ON;
    IF @CommissionPercent <= 0 OR @CommissionPercent > 100 THROW 50210, 'Commission percent must be between 0 and 100.', 1;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND IsActive = 1)
        THROW 50213, 'Active employee user not found.', 1;
    IF EXISTS (SELECT 1 FROM EmployeeCommissions WHERE UserId = @UserId AND OrderId = @OrderId)
        THROW 50211, 'Commission already exists for this employee and order.', 1;
    DECLARE @OrderAmount DECIMAL(18,2);
    SELECT @OrderAmount = GrandTotal FROM SalesOrders WHERE OrderId = @OrderId AND UPPER(Status) <> 'CANCELLED';
    IF @OrderAmount IS NULL THROW 50212, 'Eligible sales order not found.', 1;
    INSERT INTO EmployeeCommissions (UserId, OrderId, CommissionPercent, CommissionAmount)
    VALUES (@UserId, @OrderId, @CommissionPercent, ROUND(@OrderAmount * @CommissionPercent / 100.0, 2));
    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS CommissionId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeCommissions
    @UserId varchar(150) = NULL, @FromDate DATETIME2 = NULL, @ToDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ec.CommissionId, ec.UserId, u.FullName AS EmployeeName, ec.OrderId,
           so.GrandTotal AS OrderAmount, ec.CommissionPercent, ec.CommissionAmount, ec.CreatedAt
    FROM EmployeeCommissions ec INNER JOIN Users u ON u.UserId = ec.UserId
    INNER JOIN SalesOrders so ON so.OrderId = ec.OrderId
    WHERE (@UserId IS NULL OR ec.UserId = @UserId)
      AND (@FromDate IS NULL OR ec.CreatedAt >= @FromDate)
      AND (@ToDate IS NULL OR ec.CreatedAt < DATEADD(DAY, 1, CAST(@ToDate AS DATE)))
    ORDER BY ec.CreatedAt DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateInvestor
    @Name NVARCHAR(150), @Email NVARCHAR(100) = NULL, @Phone NVARCHAR(20) = NULL,
    @InvestmentAmount DECIMAL(18,2), @OwnershipPercent DECIMAL(5,2),@LossPercent DECIMAL(5,2)=null, @JoinDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @InvestmentAmount < 0 THROW 50220, 'Investment amount cannot be negative.', 1;
    IF @OwnershipPercent <= 0 OR @OwnershipPercent > 100 THROW 50221, 'Ownership percent must be between 0 and 100.', 1;
    IF @LossPercent is not null and (@LossPercent < 0 OR @LossPercent > 100) THROW 50221, 'Loss percent must be between 0 and 100.', 1;
    BEGIN TRANSACTION;
    DECLARE @CurrentOwnership DECIMAL(7,2),@CurrentLossPercent DECIMAL(7,2);;
    SELECT @CurrentOwnership = ISNULL(SUM(OwnershipPercent), 0) ,@CurrentLossPercent= ISNULL(SUM(LossPercent), 0)
    FROM Investors WITH (UPDLOCK, HOLDLOCK) WHERE Status = 1;
    IF @CurrentOwnership + @OwnershipPercent > 100 THROW 50222, 'Active investor ownership cannot exceed 100 percent.', 1;
    IF @LossPercent is not null and (@CurrentLossPercent + @LossPercent > 100) THROW 50222, 'Active investor loss cannot exceed 100 percent.', 1;
    INSERT INTO Investors (Name, Email, Phone, InvestmentAmount, OwnershipPercent,LossPercent, JoinDate, Status)
    VALUES (@Name, @Email, @Phone, @InvestmentAmount, @OwnershipPercent,@LossPercent, @JoinDate, 1);
    DECLARE @InvestorId INT = SCOPE_IDENTITY(); COMMIT TRANSACTION; SELECT @InvestorId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInvestors
AS
BEGIN
    SET NOCOUNT ON;
    SELECT InvestorId, Name, Email, Phone, InvestmentAmount, OwnershipPercent,LossPercent, JoinDate, Status
    FROM Investors ORDER BY Status DESC, Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DistributeInvestorProfit
    @Month TINYINT, @Year SMALLINT, @NetProfit DECIMAL(18,2),
    @DistributedDate DATETIME2 = NULL, @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @Month NOT BETWEEN 1 AND 12 OR @Year < 2000 THROW 50223, 'Invalid distribution period.', 1;
    IF EXISTS (SELECT 1 FROM ProfitDistributions WHERE [Month] = @Month AND [Year] = @Year)
        THROW 50224, 'Profit has already been distributed for this period.', 1;
    IF @NetProfit <= 0 THROW 50225, 'There is no positive net profit to distribute for this period.', 1;
    DECLARE @EndDate DATETIME2 = DATEADD(MONTH, 1, DATEFROMPARTS(@Year, @Month, 1));
    BEGIN TRANSACTION;
    INSERT INTO ProfitDistributions (InvestorId, [Month], [Year], ProfitAmount, DistributedDate, Notes)
    SELECT InvestorId, @Month, @Year, ROUND(@NetProfit * OwnershipPercent / 100.0, 2), @DistributedDate, @Notes
    FROM Investors WHERE Status=1 AND JoinDate<@EndDate;
    DECLARE @Count INT=@@ROWCOUNT;
    IF @Count=0 THROW 50226, 'No active investors were eligible for this period.', 1;
    COMMIT TRANSACTION; SELECT @Count;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetProfitDistributions
    @Year SMALLINT = NULL, @Month TINYINT = NULL, @InvestorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pd.DistributionId, pd.InvestorId, i.Name AS InvestorName, pd.[Month], pd.[Year],
           i.OwnershipPercent, pd.ProfitAmount, pd.DistributedDate, pd.Notes
    FROM ProfitDistributions pd JOIN Investors i ON i.InvestorId=pd.InvestorId
    WHERE (@Year IS NULL OR pd.[Year]=@Year) AND (@Month IS NULL OR pd.[Month]=@Month)
      AND (@InvestorId IS NULL OR pd.InvestorId=@InvestorId)
    ORDER BY pd.[Year] DESC, pd.[Month] DESC, i.Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetDashboardAnalytics
    @FromDate DATETIME2, @ToDate DATETIME2, @LowStockThreshold INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @FromDate>@ToDate THROW 50230, 'Invalid dashboard period.', 1;
    DECLARE @EndDate DATETIME2=DATEADD(DAY,1,CAST(@ToDate AS DATE));

    -- Dashboard shape only; all profit and ROI calculations are performed by DashboardService.
    SELECT CAST((SELECT COUNT(*) FROM SalesOrders WHERE OrderDate>=@FromDate AND OrderDate<@EndDate AND UPPER(Status)<>'CANCELLED') AS INT) AS OrderCount,
      CAST(ISNULL((SELECT AVG(GrandTotal) FROM SalesOrders WHERE OrderDate>=@FromDate AND OrderDate<@EndDate AND UPPER(Status)<>'CANCELLED'),0) AS DECIMAL(18,2)) AS AverageOrderValue,
      CAST((SELECT COUNT(*) FROM Inventory WHERE AvailableQty<=@LowStockThreshold) AS INT) AS LowStockCount;

    SELECT DATEFROMPARTS(YEAR(OrderDate),MONTH(OrderDate),1) AS Period, SUM(GrandTotal) AS Revenue, CAST(COUNT(*) AS INT) AS OrderCount,
      CAST(AVG(GrandTotal) AS DECIMAL(18,2)) AS AverageOrderValue
    FROM SalesOrders WHERE OrderDate>=@FromDate AND OrderDate<@EndDate AND UPPER(Status)<>'CANCELLED'
    GROUP BY YEAR(OrderDate),MONTH(OrderDate) ORDER BY Period;

    SELECT TOP (10) sol.VariantId,p.ProductName,pv.SKU,CAST(SUM(sol.Qty) AS INT) AS QuantitySold,
      CAST(SUM((sol.Qty*sol.UnitPrice)-sol.Discount+sol.Tax) AS DECIMAL(18,2)) AS Revenue
    FROM SalesOrders so JOIN SalesOrderLines sol ON sol.OrderId=so.OrderId JOIN ProductVariants pv ON pv.VariantId=sol.VariantId JOIN ProductMasters p ON p.ProductMasterId=pv.ProductMasterId
    WHERE so.OrderDate>=@FromDate AND so.OrderDate<@EndDate AND UPPER(so.Status)<>'CANCELLED'
    GROUP BY sol.VariantId,p.ProductName,pv.SKU ORDER BY QuantitySold DESC;

    SELECT e.ExpenseTypeId,et.Name AS ExpenseTypeName,SUM(e.Amount) AS Amount FROM Expenses e JOIN ExpenseTypes et ON et.ExpenseTypeId=e.ExpenseTypeId
    WHERE e.ExpenseDate>=@FromDate AND e.ExpenseDate<@EndDate GROUP BY e.ExpenseTypeId,et.Name ORDER BY Amount DESC;

    SELECT i.VariantId,p.ProductName,pv.SKU,i.WarehouseId,w.WarehouseName,CAST(i.AvailableQty AS INT) AS AvailableQty
    FROM Inventory i JOIN ProductVariants pv ON pv.VariantId=i.VariantId JOIN ProductMasters p ON p.ProductMasterId=pv.ProductMasterId JOIN Warehouses w ON w.WarehouseId=i.WarehouseId
    WHERE i.AvailableQty<=@LowStockThreshold ORDER BY i.AvailableQty,p.ProductName;
END;
GO
