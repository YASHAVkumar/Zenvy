CREATE OR ALTER PROCEDURE dbo.usp_CreateWarehouse
    @WarehouseName NVARCHAR(150),
    @Location NVARCHAR(255) = NULL,
    @Status BIT = 1
AS
BEGIN
    INSERT INTO Warehouses (WarehouseName, Location, Status)
    VALUES (@WarehouseName, @Location, @Status);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS WarehouseId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetWarehouses
AS
BEGIN
    SELECT WarehouseId, WarehouseName, Location, Status
    FROM Warehouses
    ORDER BY WarehouseId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetWarehouseById
    @WarehouseId INT
AS
BEGIN
    SELECT WarehouseId, WarehouseName, Location, Status
    FROM Warehouses
    WHERE WarehouseId = @WarehouseId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateWarehouse
    @WarehouseId INT,
    @WarehouseName NVARCHAR(150),
    @Location NVARCHAR(255) = NULL,
    @Status BIT
AS
BEGIN
    UPDATE Warehouses
    SET WarehouseName = @WarehouseName,
        Location = @Location,
        Status = @Status
    WHERE WarehouseId = @WarehouseId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DeleteWarehouse
    @WarehouseId INT
AS
BEGIN
    UPDATE Warehouses
    SET Status = 0
    WHERE WarehouseId = @WarehouseId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetWarehouseDropdown
AS
BEGIN
    SELECT WarehouseId, WarehouseName
    FROM Warehouses
    WHERE Status = 1
    ORDER BY WarehouseName;
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Inventory_Variant_Warehouse'
      AND object_id = OBJECT_ID('dbo.Inventory')
)
BEGIN
    CREATE UNIQUE INDEX UX_Inventory_Variant_Warehouse
    ON dbo.Inventory (VariantId, WarehouseId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateInventory
    @VariantId INT,
    @WarehouseId INT,
    @OnHandQty INT = 0,
    @ReservedQty INT = 0
AS
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM Inventory
        WHERE VariantId = @VariantId
          AND WarehouseId = @WarehouseId
    )
    BEGIN
        THROW 50001, 'Inventory already exists for this variant and warehouse.', 1;
    END;

    INSERT INTO Inventory (VariantId, WarehouseId, OnHandQty, ReservedQty)
    VALUES (@VariantId, @WarehouseId, @OnHandQty, @ReservedQty);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS InventoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInventory
AS
BEGIN
    SELECT
        i.InventoryId,
        pm.ProductMasterId,
        pm.ProductName,
        i.VariantId,
        pv.SKU,
        i.WarehouseId,
        w.WarehouseName AS Warehouse,
        i.OnHandQty,
        i.ReservedQty,
        i.AvailableQty
    FROM Inventory i
    INNER JOIN ProductVariants pv ON pv.VariantId = i.VariantId
    INNER JOIN ProductMasters pm ON pm.ProductMasterId = pv.ProductMasterId
    INNER JOIN Warehouses w ON w.WarehouseId = i.WarehouseId
    ORDER BY i.InventoryId DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInventoryById
    @InventoryId INT
AS
BEGIN
    SELECT
        i.InventoryId,
        pm.ProductMasterId,
        pm.ProductName,
        i.VariantId,
        pv.SKU,
        i.WarehouseId,
        w.WarehouseName AS Warehouse,
        i.OnHandQty,
        i.ReservedQty,
        i.AvailableQty
    FROM Inventory i
    INNER JOIN ProductVariants pv ON pv.VariantId = i.VariantId
    INNER JOIN ProductMasters pm ON pm.ProductMasterId = pv.ProductMasterId
    INNER JOIN Warehouses w ON w.WarehouseId = i.WarehouseId
    WHERE i.InventoryId = @InventoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateInventory
    @InventoryId INT,
    @VariantId INT,
    @WarehouseId INT,
    @OnHandQty INT,
    @ReservedQty INT
AS
BEGIN
    UPDATE Inventory
    SET VariantId = @VariantId,
        WarehouseId = @WarehouseId,
        OnHandQty = @OnHandQty,
        ReservedQty = @ReservedQty
    WHERE InventoryId = @InventoryId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_AdjustInventory
    @VariantId INT,
    @WarehouseId INT,
    @Quantity INT,
    @ReferenceType NVARCHAR(50) = NULL,
    @ReferenceId BIGINT = NULL,
    @CreatedBy varchar(150) = NULL
AS
BEGIN
    BEGIN TRANSACTION;

    UPDATE Inventory
    SET OnHandQty = OnHandQty + @Quantity
    WHERE VariantId = @VariantId
      AND WarehouseId = @WarehouseId;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, 'Inventory record not found.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM Inventory
        WHERE VariantId = @VariantId
          AND WarehouseId = @WarehouseId
          AND AvailableQty < 0
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50003, 'Available quantity cannot be negative.', 1;
    END;

    INSERT INTO InventoryTransactions
        (VariantId, WarehouseId, TransactionType, Quantity, ReferenceType, ReferenceId, CreatedBy)
    VALUES
        (@VariantId, @WarehouseId, 'ADJUSTMENT', @Quantity, @ReferenceType, @ReferenceId, @CreatedBy);

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_DamageInventory
    @VariantId INT,
    @WarehouseId INT,
    @Quantity INT,
    @ReferenceType NVARCHAR(50) = NULL,
    @ReferenceId BIGINT = NULL,
    @CreatedBy varchar(150) = NULL
AS
BEGIN
    IF @Quantity <= 0
    BEGIN
        THROW 50004, 'Quantity must be greater than zero.', 1;
    END;

    BEGIN TRANSACTION;

    UPDATE Inventory
    SET OnHandQty = OnHandQty - @Quantity
    WHERE VariantId = @VariantId
      AND WarehouseId = @WarehouseId
      AND AvailableQty >= @Quantity;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50005, 'Insufficient available quantity or inventory record not found.', 1;
    END;

    INSERT INTO InventoryTransactions
        (VariantId, WarehouseId, TransactionType, Quantity, ReferenceType, ReferenceId, CreatedBy)
    VALUES
        (@VariantId, @WarehouseId, 'DAMAGE', @Quantity, @ReferenceType, @ReferenceId, @CreatedBy);

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_TransferInventory
    @VariantId INT,
    @FromWarehouseId INT,
    @ToWarehouseId INT,
    @Quantity INT,
    @ReferenceType NVARCHAR(50) = NULL,
    @ReferenceId BIGINT = NULL,
    @CreatedBy varchar(150) = NULL
AS
BEGIN
    IF @Quantity <= 0
    BEGIN
        THROW 50006, 'Quantity must be greater than zero.', 1;
    END;

    IF @FromWarehouseId = @ToWarehouseId
    BEGIN
        THROW 50007, 'Source and destination warehouses must be different.', 1;
    END;

    BEGIN TRANSACTION;

    UPDATE Inventory
    SET OnHandQty = OnHandQty - @Quantity
    WHERE VariantId = @VariantId
      AND WarehouseId = @FromWarehouseId
      AND AvailableQty >= @Quantity;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50008, 'Insufficient available quantity or source inventory record not found.', 1;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM Inventory
        WHERE VariantId = @VariantId
          AND WarehouseId = @ToWarehouseId
    )
    BEGIN
        UPDATE Inventory
        SET OnHandQty = OnHandQty + @Quantity
        WHERE VariantId = @VariantId
          AND WarehouseId = @ToWarehouseId;
    END
    ELSE
    BEGIN
        INSERT INTO Inventory (VariantId, WarehouseId, OnHandQty, ReservedQty)
        VALUES (@VariantId, @ToWarehouseId, @Quantity, 0);
    END;

    INSERT INTO InventoryTransactions
        (VariantId, WarehouseId, TransactionType, Quantity, ReferenceType, ReferenceId, CreatedBy)
    VALUES
        (@VariantId, @FromWarehouseId, 'TRANSFER_OUT', @Quantity, @ReferenceType, @ReferenceId, @CreatedBy);

    INSERT INTO InventoryTransactions
        (VariantId, WarehouseId, TransactionType, Quantity, ReferenceType, ReferenceId, CreatedBy)
    VALUES
        (@VariantId, @ToWarehouseId, 'TRANSFER_IN', @Quantity, @ReferenceType, @ReferenceId, @CreatedBy);

    COMMIT TRANSACTION;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInventoryTransactions
    @VariantId INT = NULL
AS
BEGIN
    SELECT
        it.TransactionId,
        pm.ProductMasterId,
        pm.ProductName,
        it.VariantId,
        pv.SKU,
        it.WarehouseId,
        w.WarehouseName AS Warehouse,
        it.TransactionType,
        it.Quantity,
        it.ReferenceType,
        it.ReferenceId,
        it.CreatedBy,
        it.CreatedAt
    FROM InventoryTransactions it
    INNER JOIN ProductVariants pv ON pv.VariantId = it.VariantId
    INNER JOIN ProductMasters pm ON pm.ProductMasterId = pv.ProductMasterId
    INNER JOIN Warehouses w ON w.WarehouseId = it.WarehouseId
    WHERE @VariantId IS NULL
       OR it.VariantId = @VariantId
    ORDER BY it.TransactionId DESC;
END;
GO
