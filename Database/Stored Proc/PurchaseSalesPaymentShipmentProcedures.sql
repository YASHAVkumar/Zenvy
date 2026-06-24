CREATE OR ALTER PROCEDURE dbo.usp_CreateSupplier
    @Name NVARCHAR(150),
    @ContactPerson NVARCHAR(100) = NULL,
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Address NVARCHAR(255) = NULL,
    @Status BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SupplierAddress(AddressLine1,CreatedAt) VALUES(@Address,GETDATE())  
    DECLARE @SupAdsId INT=CAST(SCOPE_IDENTITY() AS INT)  
  
    INSERT INTO Suppliers (Name,AddressId,ContactPerson, Phone, Email, Status,CreatedAt)  
    VALUES (@Name,@SupAdsId,@ContactPerson, @Phone, @Email, @Status,GETDATE());

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS SupplierId;
END;
GO

CREATE or alter  PROCEDURE dbo.usp_GetSuppliers  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
     SELECT s.SupplierId, s.Name, s.ContactPerson, s.Phone, s.Email, concat(addr.AddressLine1,addr.AddressLine2) as address, s.Status, s.CreatedAt    
    FROM Suppliers s inner join SupplierAddress addr on s.AddressId=addr.AddressId    
    ORDER BY s.SupplierId DESC;   
END;  
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateCustomer
    @Name NVARCHAR(150),
    @Phone NVARCHAR(20) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Address1 NVARCHAR(255) = NULL,
    @Address2 NVARCHAR(255) = NULL,
    @City NVARCHAR(100) = NULL,
    @State NVARCHAR(100) = NULL,
    @Pincode NVARCHAR(20) = NULL,
    @Country NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Customers (Name, Phone, Email, Address1, Address2, City, State, Pincode, Country)
    VALUES (@Name, @Phone, @Email, @Address1, @Address2, @City, @State, @Pincode, @Country);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS CustomerId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetCustomers
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CustomerId, Name, Phone, Email, Address1, Address2, City, State, Pincode, Country, CreatedAt
    FROM Customers
    ORDER BY CustomerId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreatePurchaseOrder
    @SupplierId INT,
    @WarehouseId INT,
    @PONumber NVARCHAR(50),
    @OrderDate DATETIME2,
    @ExpectedDate DATETIME2 = NULL,
    @Status NVARCHAR(50),
    @CreatedBy varchar(150),
    @LinesJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @CreatedBy AND IsActive = 1)
        THROW 50104, 'Active creating user not found.', 1;

    DECLARE @Lines TABLE
    (
        VariantId INT NOT NULL,
        Qty INT NOT NULL,
        UnitCost DECIMAL(18,2) NOT NULL,
        TaxAmount DECIMAL(18,2) NOT NULL
    );

    INSERT INTO @Lines (VariantId, Qty, UnitCost, TaxAmount)
    SELECT VariantId, Qty, UnitCost, ISNULL(TaxAmount, 0)
    FROM OPENJSON(@LinesJson)
    WITH
    (
        VariantId INT '$.VariantId',
        Qty INT '$.Qty',
        UnitCost DECIMAL(18,2) '$.UnitCost',
        TaxAmount DECIMAL(18,2) '$.TaxAmount'
    );

    IF NOT EXISTS (SELECT 1 FROM @Lines)
        THROW 50100, 'Purchase order must contain at least one line.', 1;

    IF EXISTS (SELECT 1 FROM @Lines WHERE Qty <= 0 OR UnitCost < 0 OR TaxAmount < 0)
        THROW 50101, 'Purchase order line values are invalid.', 1;

    DECLARE @POId BIGINT;
    DECLARE @SubTotal DECIMAL(18,2);
    DECLARE @TaxAmount DECIMAL(18,2);
    DECLARE @GrandTotal DECIMAL(18,2);

    SELECT
        @SubTotal = SUM(Qty * UnitCost),
        @TaxAmount = SUM(TaxAmount),
        @GrandTotal = SUM((Qty * UnitCost) + TaxAmount)
    FROM @Lines;

    BEGIN TRANSACTION;

    INSERT INTO PurchaseOrders
        (SupplierId, WarehouseId, PONumber, OrderDate, ExpectedDate, Status, SubTotal, TaxAmount, GrandTotal, CreatedBy)
    VALUES
        (@SupplierId, @WarehouseId, @PONumber, @OrderDate, @ExpectedDate, @Status, @SubTotal, @TaxAmount, @GrandTotal, @CreatedBy);

    SET @POId = SCOPE_IDENTITY();

    INSERT INTO PurchaseOrderLines (POId, VariantId, Qty, UnitCost, TaxAmount)
    SELECT @POId, VariantId, Qty, UnitCost, TaxAmount
    FROM @Lines;

    MERGE Inventory AS target
    USING
    (
        SELECT VariantId, SUM(Qty) AS Qty
        FROM @Lines
        GROUP BY VariantId
    ) AS source
        ON target.VariantId = source.VariantId
       AND target.WarehouseId = @WarehouseId
    WHEN MATCHED THEN
        UPDATE SET OnHandQty = target.OnHandQty + source.Qty
    WHEN NOT MATCHED THEN
        INSERT (VariantId, WarehouseId, OnHandQty, ReservedQty)
        VALUES (source.VariantId, @WarehouseId, source.Qty, 0);

    INSERT INTO InventoryTransactions
        (VariantId, WarehouseId, TransactionType, Quantity, ReferenceType, ReferenceId, CreatedBy)
    SELECT VariantId, @WarehouseId, 'PURCHASE', SUM(Qty), 'PURCHASE_ORDER', @POId, @CreatedBy
    FROM @Lines
    GROUP BY VariantId;

    COMMIT TRANSACTION;

    SELECT @POId AS POId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPurchaseOrders
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        po.POId,
        po.SupplierId,
        s.Name AS SupplierName,
        po.WarehouseId,
        w.WarehouseName,
        po.PONumber,
        po.OrderDate,
        po.ExpectedDate,
        po.Status,
        po.SubTotal,
        po.TaxAmount,
        po.GrandTotal,
        po.CreatedBy,
        po.CreatedAt,

        ISNULL(
        (
            SELECT
                pol.POLineId,
                pol.POId,
                pm.ProductMasterId,
                pm.ProductName,
                pv.VariantId,
                pv.SKU,
                pol.Qty,
                pol.UnitCost,
                pol.TaxAmount,
                CAST(pol.LineTotal AS DECIMAL(18,2)) AS LineTotal
            FROM PurchaseOrderLines pol
            INNER JOIN ProductVariants pv
                ON pol.VariantId = pv.VariantId
            INNER JOIN ProductMasters pm
                ON pv.ProductMasterId = pm.ProductMasterId
            WHERE pol.POId = po.POId
            FOR JSON PATH
        ), '[]') AS LinesJson

    FROM PurchaseOrders po
    INNER JOIN Suppliers s
        ON s.SupplierId = po.SupplierId
    INNER JOIN Warehouses w
        ON w.WarehouseId = po.WarehouseId
    ORDER BY po.POId DESC;
END
GO

GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPurchaseOrderById 
    @POId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        po.POId,
        po.SupplierId,
        s.Name AS SupplierName,
        po.WarehouseId,
        w.WarehouseName,
        po.PONumber,
        po.OrderDate,
        po.ExpectedDate,
        po.Status,
        po.SubTotal,
        po.TaxAmount,
        po.GrandTotal,
        po.CreatedBy,
        po.CreatedAt,
        ISNULL(
        (
            SELECT
            pol.POLineId,
            pol.POId,
            pm.ProductMasterId,
            pm.ProductName,
            pol.VariantId,
            pv.SKU,
            pol.Qty,
            pol.UnitCost,
            pol.TaxAmount,
            CAST(pol.LineTotal AS DECIMAL(18,2)) AS LineTotal
            FROM PurchaseOrderLines pol
            INNER JOIN ProductVariants pv ON pv.VariantId = pol.VariantId
            INNER JOIN ProductMasters pm ON pm.ProductMasterId = pv.ProductMasterId
            WHERE pol.POId = @POId
            ORDER BY pol.POLineId
             FOR JSON PATH
          ), '[]') AS LinesJson
    FROM PurchaseOrders po
    INNER JOIN Suppliers s ON s.SupplierId = po.SupplierId
    INNER JOIN Warehouses w ON w.WarehouseId = po.WarehouseId
    WHERE po.POId = @POId
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateSalesOrder
    @CustomerId INT = NULL,
    @ChannelId INT,
    @CreatedBy varchar(150),
    @ExternalOrderId NVARCHAR(100) = NULL,
    @OrderDate DATETIME2,
    @Status NVARCHAR(50),
    @ShippingFee DECIMAL(18,2) = 0,
    @LinesJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @CreatedBy AND IsActive = 1)
        THROW 50114, 'Active creating user not found.', 1;

    DECLARE @Lines TABLE
    (
        VariantId INT NOT NULL,
        Qty INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Discount DECIMAL(18,2) NOT NULL,
        Tax DECIMAL(18,2) NOT NULL
    );

    INSERT INTO @Lines (VariantId, Qty, UnitPrice, Discount, Tax)
    SELECT VariantId, Qty, UnitPrice, ISNULL(Discount, 0), ISNULL(Tax, 0)
    FROM OPENJSON(@LinesJson)
    WITH
    (
        VariantId INT '$.VariantId',
        Qty INT '$.Qty',
        UnitPrice DECIMAL(18,2) '$.UnitPrice',
        Discount DECIMAL(18,2) '$.Discount',
        Tax DECIMAL(18,2) '$.Tax'
    );

    IF NOT EXISTS (SELECT 1 FROM @Lines)
        THROW 50110, 'Sales order must contain at least one line.', 1;

    IF EXISTS (SELECT 1 FROM @Lines WHERE Qty <= 0 OR UnitPrice < 0 OR Discount < 0 OR Tax < 0)
        THROW 50111, 'Sales order line values are invalid.', 1;

    DECLARE @Sold TABLE
    (
        VariantId INT NOT NULL PRIMARY KEY,
        Qty INT NOT NULL
    );

    DECLARE @Allocated TABLE
    (
        VariantId INT NOT NULL PRIMARY KEY,
        WarehouseId INT NOT NULL,
        Qty INT NOT NULL
    );

    INSERT INTO @Sold (VariantId, Qty)
    SELECT VariantId, SUM(Qty)
    FROM @Lines
    GROUP BY VariantId;

    INSERT INTO @Allocated (VariantId, WarehouseId, Qty)
    SELECT sold.VariantId, stock.WarehouseId, sold.Qty
    FROM @Sold sold
    CROSS APPLY
    (
        SELECT TOP 1 i.WarehouseId
        FROM Inventory i
        WHERE i.VariantId = sold.VariantId
          AND i.AvailableQty >= sold.Qty
        ORDER BY i.WarehouseId
    ) stock;

    IF (SELECT COUNT(*) FROM @Allocated) <> (SELECT COUNT(*) FROM @Sold)
        THROW 50112, 'Insufficient available quantity for one or more order lines.', 1;

    DECLARE @OrderId BIGINT;
    DECLARE @SubTotal DECIMAL(18,2);
    DECLARE @Discount DECIMAL(18,2);
    DECLARE @Tax DECIMAL(18,2);
    DECLARE @GrandTotal DECIMAL(18,2);

    SELECT
        @SubTotal = SUM(Qty * UnitPrice),
        @Discount = SUM(Discount),
        @Tax = SUM(Tax),
        @GrandTotal = SUM((Qty * UnitPrice) - Discount + Tax) + @ShippingFee
    FROM @Lines;

    BEGIN TRANSACTION;

    INSERT INTO SalesOrders
        (CustomerId, ChannelId, CreatedBy, ExternalOrderId, OrderDate, Status, SubTotal, Discount, Tax, ShippingFee, GrandTotal)
    VALUES
        (@CustomerId, @ChannelId, @CreatedBy, @ExternalOrderId, @OrderDate, @Status, @SubTotal, @Discount, @Tax, @ShippingFee, @GrandTotal);

    SET @OrderId = SCOPE_IDENTITY();

    INSERT INTO SalesOrderLines (OrderId, VariantId, Qty, UnitPrice, Discount, Tax)
    SELECT @OrderId, VariantId, Qty, UnitPrice, Discount, Tax
    FROM @Lines;

    UPDATE i
    SET OnHandQty = i.OnHandQty - allocated.Qty
    FROM Inventory i
    INNER JOIN @Allocated allocated ON allocated.VariantId = i.VariantId
                                   AND allocated.WarehouseId = i.WarehouseId
    WHERE i.AvailableQty >= allocated.Qty;

    IF @@ROWCOUNT <> (SELECT COUNT(*) FROM @Allocated)
        THROW 50113, 'Insufficient available quantity for one or more order lines.', 1;

    INSERT INTO InventoryTransactions
        (VariantId, WarehouseId, TransactionType, Quantity, ReferenceType, ReferenceId, CreatedBy)
    SELECT VariantId, WarehouseId, 'SALE', Qty, 'SALES_ORDER', @OrderId, @CreatedBy
    FROM @Allocated;

    COMMIT TRANSACTION;

    SELECT @OrderId AS OrderId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalesOrders
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        so.OrderId,
        so.CustomerId,
        c.Name AS CustomerName,
        so.ChannelId,
        sc.ChannelName,
        so.CreatedBy,
        so.ExternalOrderId,
        so.OrderDate,
        so.Status,
        so.SubTotal,
        so.Discount,
        so.Tax,
        so.ShippingFee,
        so.GrandTotal,
        so.CreatedAt
    FROM SalesOrders so
    LEFT JOIN Customers c ON c.CustomerId = so.CustomerId
    INNER JOIN SalesChannels sc ON sc.ChannelId = so.ChannelId
    ORDER BY so.OrderId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalesOrderById
    @OrderId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        so.OrderId,
        so.CustomerId,
        c.Name AS CustomerName,
        so.ChannelId,
        sc.ChannelName,
        so.CreatedBy,
        so.ExternalOrderId,
        so.OrderDate,
        so.Status,
        so.SubTotal,
        so.Discount,
        so.Tax,
        so.ShippingFee,
        so.GrandTotal,
        so.CreatedAt
    FROM SalesOrders so
    LEFT JOIN Customers c ON c.CustomerId = so.CustomerId
    INNER JOIN SalesChannels sc ON sc.ChannelId = so.ChannelId
    WHERE so.OrderId = @OrderId;

    SELECT
        sol.OrderLineId,
        sol.OrderId,
        pm.ProductMasterId,
        pm.ProductName,
        sol.VariantId,
        pv.SKU,
        sol.Qty,
        sol.UnitPrice,
        sol.Discount,
        sol.Tax,
        CAST(sol.LineTotal AS DECIMAL(18,2)) AS LineTotal
    FROM SalesOrderLines sol
    INNER JOIN ProductVariants pv ON pv.VariantId = sol.VariantId
    INNER JOIN ProductMasters pm ON pm.ProductMasterId = pv.ProductMasterId
    WHERE sol.OrderId = @OrderId
    ORDER BY sol.OrderLineId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreatePayment
    @OrderId BIGINT,
    @PaymentMethodId INT,
    @Amount DECIMAL(18,2),
    @TransactionRef NVARCHAR(200) = NULL,
    @Status NVARCHAR(50) = NULL,
    @PaymentDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Payments (OrderId, PaymentMethodId, Amount, TransactionRef, Status, PaymentDate)
    VALUES (@OrderId, @PaymentMethodId, @Amount, @TransactionRef, @Status, @PaymentDate);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS PaymentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPayments
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.PaymentId,
        p.OrderId,
        p.PaymentMethodId,
        pm.MethodName,
        p.Amount,
        p.TransactionRef,
        p.Status,
        p.PaymentDate
    FROM Payments p
    INNER JOIN PaymentMethods pm ON pm.PaymentMethodId = p.PaymentMethodId
    ORDER BY p.PaymentId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateShipment
    @OrderId BIGINT,
    @CourierName NVARCHAR(100) = NULL,
    @TrackingNumber NVARCHAR(100) = NULL,
    @ShippedDate DATETIME2 = NULL,
    @DeliveredDate DATETIME2 = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Shipments (OrderId, CourierName, TrackingNumber, ShippedDate, DeliveredDate, Status)
    VALUES (@OrderId, @CourierName, @TrackingNumber, @ShippedDate, @DeliveredDate, @Status);

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS ShipmentId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetShipments
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ShipmentId, OrderId, CourierName, TrackingNumber, ShippedDate, DeliveredDate, Status
    FROM Shipments
    ORDER BY ShipmentId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalesChannels
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ChannelId,
        ChannelName,
        ChannelType,
        Status
    FROM SalesChannels
    ORDER BY ChannelName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalesChannelById
    @ChannelId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ChannelId,
        ChannelName,
        ChannelType,
        Status
    FROM SalesChannels
    WHERE ChannelId = @ChannelId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateSalesChannel
    @ChannelName NVARCHAR(100),
    @ChannelType NVARCHAR(50) = NULL,
    @Status BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(ISNULL(@ChannelName, '')) = 0
        THROW 50160, 'Channel name is required.', 1;

    INSERT INTO SalesChannels (ChannelName, ChannelType, Status)
    VALUES (@ChannelName, @ChannelType, @Status);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ChannelId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateSalesChannel
    @ChannelId INT,
    @ChannelName NVARCHAR(100),
    @ChannelType NVARCHAR(50) = NULL,
    @Status BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM SalesChannels WHERE ChannelId = @ChannelId)
        THROW 50161, 'Sales channel not found.', 1;

    UPDATE SalesChannels
    SET ChannelName = @ChannelName,
        ChannelType = @ChannelType,
        Status = @Status
    WHERE ChannelId = @ChannelId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteSalesChannel
    @ChannelId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM SalesChannels WHERE ChannelId = @ChannelId)
        THROW 50162, 'Sales channel not found.', 1;

    DELETE FROM SalesChannels
    WHERE ChannelId = @ChannelId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateMarketplaceSettlement
    @OrderId BIGINT,
    @ChannelId INT,
    @SaleAmount DECIMAL(18,2),
    @MarketplaceCommission DECIMAL(18,2),
    @ShippingCharge DECIMAL(18,2) = 0,
    @TaxDeduction DECIMAL(18,2) = 0,
    @Status NVARCHAR(50) = 'PENDING',
    @SettlementDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM SalesOrders WHERE OrderId = @OrderId)
        THROW 50150, 'Sales order not found.', 1;

    IF NOT EXISTS (SELECT 1 FROM SalesChannels WHERE ChannelId = @ChannelId)
        THROW 50151, 'Sales channel not found.', 1;

    DECLARE @OrderChannelId INT;
    SELECT @OrderChannelId = ChannelId FROM SalesOrders WHERE OrderId = @OrderId;

    IF @OrderChannelId <> @ChannelId
        THROW 50152, 'Marketplace settlement channel does not match the sales order channel.', 1;

    DECLARE @NetReceived DECIMAL(18,2) =
        @SaleAmount
        - @MarketplaceCommission
        - @ShippingCharge
        - @TaxDeduction;

    IF @NetReceived < 0
        THROW 50153, 'Marketplace settlement net amount cannot be negative.', 1;

    INSERT INTO MarketplaceSettlements
    (
        OrderId,
        ChannelId,
        SaleAmount,
        MarketplaceCommission,
        ShippingCharge,
        TaxDeduction,
        NetReceived,
        SettlementDate,
        Status
    )
    VALUES
    (
        @OrderId,
        @ChannelId,
        @SaleAmount,
        @MarketplaceCommission,
        @ShippingCharge,
        @TaxDeduction,
        @NetReceived,
        ISNULL(@SettlementDate, GETDATE()),
        @Status
    );

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS SettlementId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetMarketplaceSettlements
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ms.SettlementId,
        ms.OrderId,
        so.ExternalOrderId,
        so.OrderDate,
        so.GrandTotal AS OrderGrandTotal,
        ms.ChannelId,
        sc.ChannelName,
        cust.Name AS CustomerName,
        ms.SaleAmount,
        ms.MarketplaceCommission,
        ms.ShippingCharge,
        ms.TaxDeduction,
        ms.NetReceived,
        ms.SettlementDate,
        ms.Status
    FROM MarketplaceSettlements ms
    INNER JOIN SalesOrders so ON so.OrderId = ms.OrderId
    INNER JOIN SalesChannels sc ON sc.ChannelId = ms.ChannelId
    LEFT JOIN Customers cust ON cust.CustomerId = so.CustomerId
    ORDER BY ms.SettlementDate DESC, ms.SettlementId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetMarketplaceSettlementById
    @SettlementId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ms.SettlementId,
        ms.OrderId,
        so.ExternalOrderId,
        so.OrderDate,
        so.GrandTotal AS OrderGrandTotal,
        ms.ChannelId,
        sc.ChannelName,
        cust.Name AS CustomerName,
        ms.SaleAmount,
        ms.MarketplaceCommission,
        ms.ShippingCharge,
        ms.TaxDeduction,
        ms.NetReceived,
        ms.SettlementDate,
        ms.Status
    FROM MarketplaceSettlements ms
    INNER JOIN SalesOrders so ON so.OrderId = ms.OrderId
    INNER JOIN SalesChannels sc ON sc.ChannelId = ms.ChannelId
    LEFT JOIN Customers cust ON cust.CustomerId = so.CustomerId
    WHERE ms.SettlementId = @SettlementId;
END;
GO

IF COL_LENGTH('SalesReturns', 'RefundStatus') IS NULL
    ALTER TABLE SalesReturns ADD RefundStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_SalesReturns_RefundStatus DEFAULT 'PENDING';
GO

IF COL_LENGTH('SalesReturns', 'RefundMethod') IS NULL
    ALTER TABLE SalesReturns ADD RefundMethod NVARCHAR(50) NULL;
GO

IF COL_LENGTH('SalesReturns', 'RefundAmount') IS NULL
    ALTER TABLE SalesReturns ADD RefundAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturns_RefundAmount DEFAULT 0;
GO

IF COL_LENGTH('SalesReturns', 'ReturnShippingFee') IS NULL
    ALTER TABLE SalesReturns ADD ReturnShippingFee DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturns_ReturnShippingFee DEFAULT 0;
GO

IF COL_LENGTH('SalesReturns', 'MarketplaceFee') IS NULL
    ALTER TABLE SalesReturns ADD MarketplaceFee DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturns_MarketplaceFee DEFAULT 0;
GO

IF COL_LENGTH('SalesReturns', 'DeliveryFeeRefunded') IS NULL
    ALTER TABLE SalesReturns ADD DeliveryFeeRefunded BIT NOT NULL CONSTRAINT DF_SalesReturns_DeliveryFeeRefunded DEFAULT 0;
GO

IF COL_LENGTH('SalesReturns', 'CreatedBy') IS NULL
    ALTER TABLE SalesReturns ADD CreatedBy varchar(150) NULL;
GO

IF COL_LENGTH('SalesReturns', 'Notes') IS NULL
    ALTER TABLE SalesReturns ADD Notes NVARCHAR(500) NULL;
GO

IF COL_LENGTH('SalesReturns', 'CreatedAt') IS NULL
    ALTER TABLE SalesReturns ADD CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_SalesReturns_CreatedAt DEFAULT GETDATE();
GO

IF COL_LENGTH('SalesReturnLines', 'OrderLineId') IS NULL
    ALTER TABLE SalesReturnLines ADD OrderLineId BIGINT NULL;
GO

IF COL_LENGTH('SalesReturnLines', 'RefundAmount') IS NULL
    ALTER TABLE SalesReturnLines ADD RefundAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_SalesReturnLines_RefundAmount DEFAULT 0;
GO

IF COL_LENGTH('SalesReturnLines', 'Condition') IS NULL
    ALTER TABLE SalesReturnLines ADD [Condition] NVARCHAR(50) NULL;
GO

IF COL_LENGTH('SalesReturnLines', 'Restock') IS NULL
    ALTER TABLE SalesReturnLines ADD Restock BIT NOT NULL CONSTRAINT DF_SalesReturnLines_Restock DEFAULT 1;
GO

IF COL_LENGTH('SalesReturnLines', 'RestockWarehouseId') IS NULL
    ALTER TABLE SalesReturnLines ADD RestockWarehouseId INT NULL;
GO
CREATE OR ALTER PROCEDURE dbo.usp_CreateSalesReturn
    @OrderId BIGINT,
    @ReturnDate DATETIME2,
    @Reason NVARCHAR(500) = NULL,
    @Status NVARCHAR(50) = 'REQUESTED',
    @RefundStatus NVARCHAR(50) = 'PENDING',
    @RefundMethod NVARCHAR(50) = NULL,
    @RefundAmount DECIMAL(18,2) = 0,
    @ReturnShippingFee DECIMAL(18,2) = 0,
    @MarketplaceFee DECIMAL(18,2) = 0,
    @DeliveryFeeRefunded BIT = 0,
    @CreatedBy VARCHAR(150) = NULL,
    @Notes NVARCHAR(500) = NULL,
    @LinesJson NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @CreatedBy IS NOT NULL
       AND NOT EXISTS
       (
            SELECT 1
            FROM Users
            WHERE UserId = @CreatedBy
              AND IsActive = 1
       )
        THROW 50124, 'Active creating user not found.', 1;

    DECLARE @Lines TABLE
    (
        OrderLineId BIGINT NOT NULL,
        VariantId INT NULL,
        Qty INT NOT NULL,
        RefundAmount DECIMAL(18,2) NOT NULL,
        [Condition] NVARCHAR(50),
        Restock BIT NOT NULL,
        RestockWarehouseId INT NULL
    );

    INSERT INTO @Lines
    (
        OrderLineId,
        Qty,
        RefundAmount,
        [Condition],
        Restock,
        RestockWarehouseId
    )
    SELECT
        OrderLineId,
        Qty,
        ISNULL(RefundAmount,0),
        [Condition],
        ISNULL(Restock,1),
        RestockWarehouseId
    FROM OPENJSON(@LinesJson)
    WITH
    (
        OrderLineId BIGINT '$.OrderLineId',
        Qty INT '$.Qty',
        RefundAmount DECIMAL(18,2) '$.RefundAmount',
        [Condition] NVARCHAR(50) '$.Condition',
        Restock BIT '$.Restock',
        RestockWarehouseId INT '$.RestockWarehouseId'
    );

    IF NOT EXISTS (SELECT 1 FROM @Lines)
        THROW 50130, 'Return must contain at least one line.', 1;

    UPDATE l
    SET VariantId = sol.VariantId
    FROM @Lines l
    INNER JOIN SalesOrderLines sol
        ON sol.OrderLineId = l.OrderLineId
    WHERE sol.OrderId = @OrderId;

    DECLARE @ResolvedLines TABLE
    (
        OrderLineId BIGINT,
        VariantId INT,
        Qty INT,
        RefundAmount DECIMAL(18,2),
        [Condition] NVARCHAR(50),
        Restock BIT,
        RestockWarehouseId INT
    );

    INSERT INTO @ResolvedLines
    (
        OrderLineId,
        VariantId,
        Qty,
        RefundAmount,
        [Condition],
        Restock,
        RestockWarehouseId
    )
    SELECT
        l.OrderLineId,
        l.VariantId,
        l.Qty,
        l.RefundAmount,
        l.[Condition],
        l.Restock,
        COALESCE(l.RestockWarehouseId, stock.WarehouseId)
    FROM @Lines l
    OUTER APPLY
    (
        SELECT TOP 1 WarehouseId
        FROM InventoryTransactions
        WHERE ReferenceType = 'SALES_ORDER'
          AND ReferenceId = @OrderId
          AND VariantId = l.VariantId
        ORDER BY TransactionId
    ) stock;

    DECLARE @ReturnId BIGINT;

    BEGIN TRANSACTION;

    INSERT INTO SalesReturns
    (
        OrderId,
        ReturnDate,
        Reason,
        Status,
        RefundStatus,
        RefundMethod,
        RefundAmount,
        ReturnShippingFee,
        MarketplaceFee,
        DeliveryFeeRefunded,
        CreatedBy,
        Notes
    )
    VALUES
    (
        @OrderId,
        @ReturnDate,
        @Reason,
        @Status,
        @RefundStatus,
        @RefundMethod,
        @RefundAmount,
        @ReturnShippingFee,
        @MarketplaceFee,
        @DeliveryFeeRefunded,
        @CreatedBy,
        @Notes
    );

    SET @ReturnId = SCOPE_IDENTITY();

    INSERT INTO SalesReturnLines
    (
        ReturnId,
        OrderLineId,
        VariantId,
        Qty,
        RefundAmount,
        [Condition],
        Restock,
        RestockWarehouseId
    )
    SELECT
        @ReturnId,
        OrderLineId,
        VariantId,
        Qty,
        RefundAmount,
        [Condition],
        Restock,
        RestockWarehouseId
    FROM @ResolvedLines;

    MERGE Inventory AS target
    USING
    (
        SELECT
            VariantId,
            RestockWarehouseId AS WarehouseId,
            SUM(Qty) Qty
        FROM @ResolvedLines
        WHERE Restock = 1
        GROUP BY VariantId, RestockWarehouseId
    ) source
    ON target.VariantId = source.VariantId
    AND target.WarehouseId = source.WarehouseId

    WHEN MATCHED THEN
        UPDATE
        SET OnHandQty = target.OnHandQty + source.Qty

    WHEN NOT MATCHED THEN
        INSERT
        (
            VariantId,
            WarehouseId,
            OnHandQty,
            ReservedQty
        )
        VALUES
        (
            source.VariantId,
            source.WarehouseId,
            source.Qty,
            0
        );

    INSERT INTO InventoryTransactions
    (
        VariantId,
        WarehouseId,
        TransactionType,
        Quantity,
        ReferenceType,
        ReferenceId,
        CreatedBy
    )
    SELECT
        VariantId,
        RestockWarehouseId,
        'RETURN',
        SUM(Qty),
        'SALES_RETURN',
        @ReturnId,
        @CreatedBy
    FROM @ResolvedLines
    WHERE Restock = 1
    GROUP BY VariantId, RestockWarehouseId;

    COMMIT TRANSACTION;

    SELECT @ReturnId AS ReturnId;
END;
GO
-- =============================================
-- DASHBOARD ANALYTICS STORED PROCEDURES
-- =============================================

CREATE OR ALTER PROCEDURE dbo.usp_GetExecutiveDashboard
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TodayStart DATETIME2 = CAST(GETDATE() AS DATE);
    DECLARE @TodayEnd   DATETIME2 = DATEADD(DAY, 1, CAST(GETDATE() AS DATE));
    DECLARE @MonthStart DATETIME2 = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    DECLARE @MonthEnd   DATETIME2 = DATEADD(DAY, 1, EOMONTH(GETDATE()));

    ;WITH OrderCost AS
    (
        SELECT
            OrderId,
            SUM(Qty * UnitPrice) AS TotalCost
        FROM SalesOrderLines
        GROUP BY OrderId
    )
    SELECT
        ISNULL(SUM(CASE
            WHEN so.OrderDate >= @TodayStart
             AND so.OrderDate < @TodayEnd
            THEN so.GrandTotal
            ELSE 0
        END),0) AS TodaysSales,

        ISNULL(SUM(CASE
            WHEN so.OrderDate >= @MonthStart
             AND so.OrderDate < @MonthEnd
            THEN so.GrandTotal
            ELSE 0
        END),0) AS MonthlySales,

        ISNULL(SUM(CASE
            WHEN so.OrderDate >= @MonthStart
             AND so.OrderDate < @MonthEnd
            THEN so.GrandTotal - ISNULL(oc.TotalCost,0)
            ELSE 0
        END),0) AS MonthlyProfit,

        0 AS InventoryValue,
        0 AS AvailableCash,
        0 AS ReserveFund,

        ISNULL((SELECT SUM(InvestmentAmount)
                FROM Investors
                WHERE Status = 1),0) AS InvestorLiability,

        (SELECT COUNT(*)
         FROM SalesOrders
         WHERE Status IN ('PENDING','PROCESSING')) AS PendingOrders,

        (SELECT COUNT(*)
         FROM SalesReturns
         WHERE Status = 'REQUESTED') AS PendingReturns

    FROM SalesOrders so
    LEFT JOIN OrderCost oc
        ON so.OrderId = oc.OrderId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetFinanceDashboard
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    CAST(0 AS DECIMAL(18,2)) AS CashAvailable,
    CAST(0 AS DECIMAL(18,2)) AS BankBalance,
    CAST(0 AS DECIMAL(18,2)) AS WorkingCapital,
    CAST(0 AS DECIMAL(18,2)) AS ReserveFund,
    CAST(ISNULL(SUM(i.InvestmentAmount), 0) AS DECIMAL(18,2)) AS InvestorCapital,
    CAST(0 AS DECIMAL(18,2)) AS OutstandingLoans,
    CAST(ISNULL(SUM(so.GrandTotal), 0) AS DECIMAL(18,2)) AS Receivables,
    CAST(ISNULL(SUM(po.SubTotal), 0) AS DECIMAL(18,2)) AS Payables
    FROM Investors i
    FULL OUTER JOIN SalesOrders so ON so.OrderId IS NOT NULL
    FULL OUTER JOIN PurchaseOrders po ON po.POId IS NOT NULL;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetOperationsDashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        (SELECT COUNT(*) FROM PurchaseOrders WHERE Status = 'PENDING') AS PendingPurchases,
        0 AS PendingGoodsReceipt,
        ISNULL(SUM(il.Qty), 0) AS CurrentInventory,
        (SELECT COUNT(*) FROM SalesOrders WHERE Status IN ('PENDING', 'PROCESSING')) AS PendingOrders,
        (SELECT COUNT(*) FROM SalesOrders WHERE Status = 'PACKED') AS PendingDispatch,
        (SELECT COUNT(*) FROM SalesReturns WHERE Status = 'REQUESTED') AS Returns,
        0 AS DamagedStock
    FROM InventoryLedger il;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetMarketplaceDashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        (SELECT COUNT(*) FROM SalesOrders WHERE Status IS NOT NULL) AS TotalOrders,
        (SELECT COUNT(*) FROM SalesOrders WHERE Status = 'DELIVERED') AS DeliveredOrders,
        (SELECT COUNT(*) FROM SalesOrders WHERE Status = 'CANCELLED') AS CancelledOrders,
        (SELECT COUNT(*) FROM SalesReturns WHERE Status != 'REJECTED') AS ReturnedOrders,
        (SELECT COUNT(*) FROM MarketplaceSettlements WHERE Status = 'PENDING') AS SettlementPending,
        ISNULL(SUM(CASE WHEN ms.Status = 'SETTLED' THEN ms.NetReceived ELSE 0 END), 0) AS SettlementReceived
    FROM MarketplaceSettlements ms;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInvestorDashboard
    @InvestorId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        i.InvestorId,
        i.Name,
        ISNULL(SUM(inv.InvestmentAmount), 0) AS TotalInvested,
        ISNULL(SUM(inv.InvestmentAmount), 0) AS CurrentCapital,
        0 AS ProfitEarned,
        0 AS ProfitPaid,
        0 AS PendingProfit,
        0 AS RoiPercent
    FROM Investors i
    LEFT JOIN Investments inv ON i.InvestorId = inv.InvestorId
    WHERE i.InvestorId = @InvestorId
    GROUP BY i.InvestorId, i.Name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeDashboard
    @EmployeeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.EmployeeId,
        e.EmployeeName,
        (SELECT COUNT(*) FROM Tasks WHERE AssignedTo = e.EmployeeId AND Status != 'COMPLETED') AS TasksAssigned,
        (SELECT COUNT(*) FROM Tasks WHERE AssignedTo = e.EmployeeId AND Status = 'COMPLETED') AS TasksCompleted,
        (SELECT COUNT(DISTINCT AttendanceDate) FROM Attendance WHERE EmployeeId = e.EmployeeId AND Status = 'PRESENT') AS AttendanceDays,
        ISNULL(sal.BaseSalary, 0) AS Salary,
        ISNULL(SUM(ec.CommissionAmount), 0) AS Commission,
        0 AS PerformanceScore
    FROM Employees e
    LEFT JOIN Salary sal ON e.EmployeeId = sal.EmployeeId AND sal.IsActive = 1
    LEFT JOIN EmployeeCommission ec ON e.EmployeeId = ec.EmployeeId
    WHERE @EmployeeId IS NULL OR e.EmployeeId = @EmployeeId
    GROUP BY e.EmployeeId, e.EmployeeName, sal.BaseSalary;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetChannelAnalytics
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sc.ChannelName,

        ISNULL(SUM(so.GrandTotal), 0) AS ChannelRevenue,

        ISNULL(
            SUM(so.GrandTotal) -
            SUM(ISNULL(sol.OrderCost, 0)),
            0
        ) AS ChannelProfit,

        CASE
            WHEN SUM(so.GrandTotal) > 0
            THEN (SUM(ISNULL(ms.MarketplaceCommission, 0)) * 100.0)
                 / SUM(so.GrandTotal)
            ELSE 0
        END AS CommissionPercent,

        CASE
            WHEN COUNT(DISTINCT so.OrderId) > 0
            THEN (COUNT(DISTINCT sr.ReturnId) * 100.0)
                 / COUNT(DISTINCT so.OrderId)
            ELSE 0
        END AS ReturnPercent,

        0 AS PenaltyPercent

    FROM SalesChannels sc

    LEFT JOIN SalesOrders so
        ON sc.ChannelId = so.ChannelId

    LEFT JOIN
    (
        SELECT
            OrderId,
            SUM(Qty * UnitPrice) AS OrderCost
        FROM SalesOrderLines
        GROUP BY OrderId
    ) sol
        ON so.OrderId = sol.OrderId

    LEFT JOIN MarketplaceSettlements ms
        ON so.OrderId = ms.OrderId

    LEFT JOIN SalesReturns sr
        ON so.OrderId = sr.OrderId

    GROUP BY sc.ChannelName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalesReturns
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sr.ReturnId,
        sr.OrderId,
        so.ChannelId,
        sc.ChannelName,
        so.ExternalOrderId,
        so.CustomerId,
        c.Name AS CustomerName,
        sr.ReturnDate,
        sr.Reason,
        ISNULL(sr.Status, '') AS Status,
        sr.RefundStatus,
        sr.RefundMethod,
        sr.RefundAmount,
        sr.ReturnShippingFee,
        sr.MarketplaceFee,
        sr.DeliveryFeeRefunded,
        sr.CreatedBy,
        sr.Notes,
        sr.CreatedAt
    FROM SalesReturns sr
    INNER JOIN SalesOrders so ON so.OrderId = sr.OrderId
    INNER JOIN SalesChannels sc ON sc.ChannelId = so.ChannelId
    LEFT JOIN Customers c ON c.CustomerId = so.CustomerId
    ORDER BY sr.ReturnId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetSalesReturnById
    @ReturnId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        sr.ReturnId,
        sr.OrderId,
        so.ChannelId,
        sc.ChannelName,
        so.ExternalOrderId,
        so.CustomerId,
        c.Name AS CustomerName,
        sr.ReturnDate,
        sr.Reason,
        ISNULL(sr.Status, '') AS Status,
        sr.RefundStatus,
        sr.RefundMethod,
        sr.RefundAmount,
        sr.ReturnShippingFee,
        sr.MarketplaceFee,
        sr.DeliveryFeeRefunded,
        sr.CreatedBy,
        sr.Notes,
        sr.CreatedAt
    FROM SalesReturns sr
    INNER JOIN SalesOrders so ON so.OrderId = sr.OrderId
    INNER JOIN SalesChannels sc ON sc.ChannelId = so.ChannelId
    LEFT JOIN Customers c ON c.CustomerId = so.CustomerId
    WHERE sr.ReturnId = @ReturnId;

    SELECT
        srl.ReturnLineId,
        srl.ReturnId,
        srl.OrderLineId,
        pm.ProductMasterId,
        srl.VariantId,
        pv.SKU,
        pm.ProductName,
        srl.Qty,
        srl.RefundAmount,
        srl.[Condition],
        srl.Restock,
        srl.RestockWarehouseId,
        w.WarehouseName AS RestockWarehouseName
    FROM SalesReturnLines srl
    INNER JOIN ProductVariants pv ON pv.VariantId = srl.VariantId
    INNER JOIN ProductMasters pm ON pm.ProductMasterId = pv.ProductMasterId
    LEFT JOIN Warehouses w ON w.WarehouseId = srl.RestockWarehouseId
    WHERE srl.ReturnId = @ReturnId
    ORDER BY srl.ReturnLineId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateSalesReturnStatus
    @ReturnId BIGINT,
    @Status NVARCHAR(50),
    @RefundStatus NVARCHAR(50) = NULL,
    @Notes NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE SalesReturns
    SET Status = @Status,
        RefundStatus = COALESCE(@RefundStatus, RefundStatus),
        Notes = COALESCE(@Notes, Notes)
    WHERE ReturnId = @ReturnId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO
