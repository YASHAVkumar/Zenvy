CREATE or alter  PROCEDURE usp_CreateProduct  
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
    @Images ProductImageType READONLY,
    @VariantPrice ProductVariantPriceType READONLY
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
        
        DECLARE @VariantMapping TABLE
            (
                SKU NVARCHAR(150),
                VariantId INT
            );

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
            Status  
        )
        OUTPUT
        inserted.SKU,
        inserted.VariantId
        INTO @VariantMapping(SKU, VariantId)
        SELECT  
            @ProductMasterId,  
            @ProductId,  
            src.SKU,  
            Barcode,  
            src.Size,  
            Color,  
            Material,  
            Gender,  
            Season,    
            Status  
        FROM @Variants src;  
  
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
        
        INSERT INTO ProductVariantPrice  
        (   
            [VariantId]
           ,[ProductId]
           ,[CostPrice]
           ,[SalePrice]
           ,[EffectiveFrom]
           ,[EffectiveTo]
           ,[PlatformFee]
           ,[Discount]
           ,[DiscountType]
           ,[CreatedBy]
           ,[CreatedOn]
           )  
        SELECT  
            vm.VariantId
           ,@ProductId
           ,CostPrice
           ,SalePrice
           ,EffectiveFrom
           ,EffectiveTo
           ,PlatformFee
           ,Discount
           ,DiscountType
           ,isNull(@CreatedBy,'5f38cf57-f3f5-4ef4-8554-d8678f9ed7b')    
           ,GETDATE()  
        FROM @VariantPrice vp
        JOIN @VariantMapping vm
         ON vp.SKU = vm.SKU;

  
        COMMIT TRANSACTION;  
  
        SELECT @ProductMasterId AS ProductMasterId;  
    END TRY  
    BEGIN CATCH  
        IF @@TRANCOUNT > 0  
            ROLLBACK TRANSACTION;  
  
        THROW;  
    END CATCH  
END  