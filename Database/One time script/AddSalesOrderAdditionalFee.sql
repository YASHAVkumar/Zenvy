IF COL_LENGTH('dbo.SalesOrders', 'AdditionalFee') IS NULL
BEGIN
    ALTER TABLE dbo.SalesOrders
        ADD AdditionalFee DECIMAL(18, 2) NOT NULL
            CONSTRAINT DF_SalesOrders_AdditionalFee DEFAULT (0);
END;
GO
