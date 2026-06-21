CREATE OR ALTER PROCEDURE usp_CreateProduct
(
    @ProductCode NVARCHAR(50),
    @ProductName NVARCHAR(250),
    @CategoryId INT,
    @BrandId INT = NULL,
    @Description NVARCHAR(MAX) = NULL,
    @HSNCode NVARCHAR(20) = NULL,
    @GSTPercentage DECIMAL(5,2),
    @CreatedBy varchar(150) = NULL,
    @Variants ProductVariantType READONLY,
    @Images ProductImageType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ProductMasterId INT;
        DECLARE @ProductId INT;

        INSERT INTO ProductMasters
        (
            ProductCode,
            ProductName,
            CategoryId,
            BrandId,
            Description,
            HSNCode,
            GSTPercentage,
            IsActive,
            CreatedBy,
            CreatedDate
        )
        VALUES
        (
            @ProductCode,
            @ProductName,
            @CategoryId,
            @BrandId,
            @Description,
            @HSNCode,
            @GSTPercentage,
            1,
            @CreatedBy,
            GETDATE()
        );

        SET @ProductMasterId = SCOPE_IDENTITY();

        INSERT INTO Products
        (
            ProductMasterId,
            IsActive,
            CreatedDate
        )
        VALUES
        (
            @ProductMasterId,
            1,
            GETDATE()
        );

        SET @ProductId = SCOPE_IDENTITY();

        INSERT INTO ProductVariants
        (
            ProductMasterId,
            ProductId,
            SKU,
            Barcode,
            Size,
            Color,
            Material,
            Gender,
            Season,
            CurrentPrice,
            Status
        )
        SELECT
            @ProductMasterId,
            @ProductId,
            SKU,
            Barcode,
            Size,
            Color,
            Material,
            Gender,
            Season,
            CurrentPrice,
            Status
        FROM @Variants;

        INSERT INTO ProductImages
        (
            ProductMasterId,
            ProductId,
            ImageUrl,
            IsPrimary,
            CreatedDate
        )
        SELECT
            @ProductMasterId,
            @ProductId,
            ImageUrl,
            IsPrimary,
            GETDATE()
        FROM @Images;

        COMMIT TRANSACTION;

        SELECT @ProductMasterId AS ProductMasterId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
