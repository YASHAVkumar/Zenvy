using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Products;
using zenvy.application.Interfaces.Repositories;
using zenvy.domain.Entities;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository.products;

public class ProductsRepository(IConfiguration configuration) : IProductsRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<int> CreateAsync(CreateProductRequest request)
    {
        try
        {
            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = new SqlCommand("usp_CreateProduct", connection, transaction);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ProductCode", request.ProductMaster.ProductCode);
                command.Parameters.AddWithValue("@ProductName", request.ProductMaster.ProductName);
                command.Parameters.AddWithValue("@CategoryId", request.ProductMaster.CategoryId);
                command.Parameters.AddWithValue("@BrandId", (object?)request.ProductMaster.BrandId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Description", (object?)request.ProductMaster.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@HSNCode", (object?)request.ProductMaster.HSNCode ?? DBNull.Value);
                command.Parameters.AddWithValue("@GSTPercentage", request.ProductMaster.GSTPercentage);
                command.Parameters.AddWithValue("@CreatedBy", DBNull.Value);

                // Pass the incoming standard scalar cost/selling pricing for the main Product entity
                //command.Parameters.AddWithValue("@CostPrice", request.Product.CostPrice);
                //command.Parameters.AddWithValue("@SellingPrice", request.Product.SellingPrice);
                //command.Parameters.AddWithValue("@Quantity", request.Product.Quantity);

                // Structured Parameter: Variants
                var variantParam = command.Parameters.AddWithValue("@Variants", CreateVariantTable(request.ProductVariants));
                variantParam.SqlDbType = SqlDbType.Structured;
                variantParam.TypeName = "dbo.ProductVariantType";
                //variant price into table 
                var priceParam = command.Parameters.AddWithValue(
                    "@VariantPrice",
                    CreateVariantPriceTable(request.ProductVariants));

                priceParam.SqlDbType = SqlDbType.Structured;
                priceParam.TypeName = "dbo.ProductVariantPriceType";


                // Structured Parameter: Images
                var imageParam = command.Parameters.AddWithValue("@Images", CreateImageTable(request.ProductImages));
                imageParam.SqlDbType = SqlDbType.Structured;
                imageParam.TypeName = "dbo.ProductImageType";


                // CRITICAL FIX: Stored Procedure returns a result set (2 columns, 1 row), NOT a single scalar value.
                using var reader = await command.ExecuteReaderAsync();
                int productMasterId = 0;
                if (await reader.ReadAsync())
                {
                    productMasterId = Convert.ToInt32(reader["ProductMasterId"]);
                }

                reader.Close(); // Explicitly close reader before committing transaction
                transaction.Commit();
                return productMasterId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating the Product: {ex.Message}");
            throw;
        }
    }
    private static DataTable CreateVariantPriceTable(
        List<ProductVariants> variants)
    {
        var table = new DataTable();

        table.Columns.Add("SKU", typeof(string));
        table.Columns.Add("CostPrice", typeof(decimal));
        table.Columns.Add("SalePrice", typeof(decimal));
        table.Columns.Add("EffectiveFrom", typeof(DateTime));
        table.Columns.Add("EffectiveTo", typeof(DateTime));
        table.Columns.Add("PlatformFee", typeof(decimal));
        table.Columns.Add("Discount", typeof(int));
        table.Columns.Add("DiscountType", typeof(string));

        foreach (var variant in variants)
        {
            var price = variant.ProductVariantPrice;

            table.Rows.Add(
                variant.SKU,
                price.CostPrice,
                price.SalePrice,
                price.EffectiveFrom,
                price.EffectiveTo.HasValue
                    ? (object)price.EffectiveTo.Value
                    : DBNull.Value,
                price.PlatformFee,
                price.Discount,
                price.DiscountType.Trim()
            );
        }

        return table;
    }
    public async Task<IReadOnlyList<int>> BulkCreateAsync(IReadOnlyCollection<CreateProductRequest> requests)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var ids = new List<int>(requests.Count);
            foreach (var request in requests)
                ids.Add(await CreateWithinTransactionAsync(connection, transaction, request));
            transaction.Commit();
            return ids;
        }
        catch
        {
            TryRollback(transaction);
            throw;
        }
    }

    public async Task<IReadOnlyList<int>> BulkUpdateAsync(IReadOnlyCollection<BulkUpdateProductItem> requests)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            var ids = new List<int>(requests.Count);
            foreach (var item in requests)
            {
                if (!await UpdateWithinTransactionAsync(connection, transaction, item.ProductMasterId, item.Product))
                    throw new InvalidOperationException($"Product {item.ProductMasterId} was not found.");
                ids.Add(item.ProductMasterId);
            }
            transaction.Commit();
            return ids;
        }
        catch
        {
            TryRollback(transaction);
            throw;
        }
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(ProductQueryRequest request)
    {
        try
        {
            var products = new List<ProductResponse>();
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
            var offset = (pageNumber - 1) * pageSize;

            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
           SELECT
                p.ProductMasterId,
                p.ProductName,
                p.CategoryId,
                c.Name AS CategoryName,
                p.BrandId,
                b.Name AS BrandName,
                p.Description,
                p.IsActive,
                p.CreatedDate
            FROM ProductMasters p
            inner join Products pd on p.ProductMasterId=pd.ProductMasterId
            INNER JOIN Categories c ON c.CategoryId = p.CategoryId
            LEFT JOIN Brands b ON b.BrandId = p.BrandId
            WHERE (@Search IS NULL OR p.ProductName LIKE @Search OR p.Description LIKE @Search)
            AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
            AND (@BrandId IS NULL OR p.BrandId = @BrandId)
            AND (@IsActive IS NULL OR p.IsActive = @IsActive)
            ORDER BY p.ProductMasterId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;", connection);

            command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(request.Search) ? DBNull.Value : $"%{request.Search}%");
            command.Parameters.AddWithValue("@CategoryId", (object?)request.CategoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("@BrandId", (object?)request.BrandId ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", (object?)request.IsActive ?? DBNull.Value);
            command.Parameters.AddWithValue("@Offset", offset);
            command.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            reader.Close();

            foreach (var product in products)
            {
                product.ProductVariants = await GetVariantsAsync(connection, product.ProductMasterId);
                product.ProductImages = await GetImagesAsync(connection, product.ProductMasterId);
            }

            return products;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while retrieving Products: {ex.Message}");
            throw;
        }
    }

    public async Task<ProductResponse?> GetByIdAsync(int productMasterId)
    {
        try
        {
            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            ProductResponse? product = null;
            using (var command = new SqlCommand(@"
SELECT
    p.ProductMasterId,
    p.ProductName,
    p.CategoryId,
    c.Name AS CategoryName,
    p.BrandId,
    b.Name AS BrandName,
    p.Description,
    p.IsActive,
    p.CreatedDate
FROM ProductMasters p
INNER JOIN Products pd ON pd.ProductMasterId = p.ProductMasterId
INNER JOIN Categories c ON c.CategoryId = p.CategoryId
LEFT JOIN Brands b ON b.BrandId = p.BrandId
WHERE p.ProductMasterId = @ProductMasterId;", connection))
            {
                command.Parameters.AddWithValue("@ProductMasterId", productMasterId);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    product = MapProduct(reader);
                }
            }

            if (product == null) return null;

            product.ProductVariants = await GetVariantsAsync(connection, productMasterId);
            product.ProductImages = await GetImagesAsync(connection, productMasterId);
            return product;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while retrieving the Product: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(int productMasterId, CreateProductRequest request)
    {
        try
        {
            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using (var command = new SqlCommand(@"
UPDATE ProductMasters
SET
    ProductCode = @ProductCode,
    ProductName = @ProductName,
    CategoryId = @CategoryId,
    BrandId = @BrandId,
    Description = @Description,
    HSNCode = @HSNCode,
    GSTPercentage = @GSTPercentage,
    IsActive = @IsActive
WHERE ProductMasterId = @ProductMasterId;", connection, transaction))
                {
                    command.Parameters.AddWithValue("@ProductMasterId", productMasterId);
                    command.Parameters.AddWithValue("@ProductCode", request.ProductMaster.ProductCode);
                    command.Parameters.AddWithValue("@ProductName", request.ProductMaster.ProductName);
                    command.Parameters.AddWithValue("@CategoryId", request.ProductMaster.CategoryId);
                    command.Parameters.AddWithValue("@BrandId", (object?)request.ProductMaster.BrandId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object?)request.ProductMaster.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HSNCode", (object?)request.ProductMaster.HSNCode ?? DBNull.Value);
                    command.Parameters.AddWithValue("@GSTPercentage", request.ProductMaster.GSTPercentage);
                    command.Parameters.AddWithValue("@IsActive", request.ProductMaster.IsActive);

                    if (await command.ExecuteNonQueryAsync() == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                var productId = await GetProductIdAsync(connection, transaction, productMasterId);
                if (productId == null)
                {
                    transaction.Rollback();
                    return false;
                }

                using (var statusCommand = new SqlCommand("UPDATE Products SET IsActive = @IsActive WHERE ProductMasterId = @ProductMasterId", connection, transaction))
                {
                    statusCommand.Parameters.AddWithValue("@ProductMasterId", productMasterId);
                    statusCommand.Parameters.AddWithValue("@IsActive", request.ProductMaster.IsActive);
                    await statusCommand.ExecuteNonQueryAsync();
                }

                await ReplaceImagesAsync(connection, transaction, productMasterId, request.ProductImages);
                await ReplaceVariantsAsync(connection, transaction, productMasterId, request.ProductVariants);

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating the Product: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int productMasterId)
    {
        try
        {
            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
UPDATE ProductMasters SET IsActive = 0 WHERE ProductMasterId = @ProductMasterId;
UPDATE Products SET IsActive = 0 WHERE ProductMasterId = @ProductMasterId;", connection);
            command.Parameters.AddWithValue("@ProductMasterId", productMasterId);

            return await command.ExecuteNonQueryAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting the Product: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int productMasterId, bool isActive)
    {
        try
        {
            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
UPDATE ProductMasters SET IsActive = @IsActive WHERE ProductMasterId = @ProductMasterId;
UPDATE Products SET IsActive = @IsActive WHERE ProductMasterId = @ProductMasterId;", connection);
            command.Parameters.AddWithValue("@ProductMasterId", productMasterId);
            command.Parameters.AddWithValue("@IsActive", isActive);

            return await command.ExecuteNonQueryAsync() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating the Product status: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ProductDropdownResponse>> GetDropdownAsync()
    {
        try
        {
            var products = new List<ProductDropdownResponse>();

            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
SELECT ProductMasterId, ProductName
FROM ProductMasters
WHERE IsActive = 1
ORDER BY ProductName;", connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new ProductDropdownResponse
                {
                    ProductMasterId = reader.GetInt32(0),
                    ProductName = reader.GetString(1)
                });
            }

            return products;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while retrieving Product dropdown: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ProductDropdownResponse>> SearchAsync(string? search)
    {
        try
        {
            var products = new List<ProductDropdownResponse>();

            using var connection = new SqlConnection(sqlConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
SELECT ProductMasterId, ProductName
FROM ProductMasters
WHERE (@Search IS NULL OR ProductName LIKE @Search)
ORDER BY ProductName;", connection);

            command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : $"%{search}%");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new ProductDropdownResponse
                {
                    ProductMasterId = reader.GetInt32(0),
                    ProductName = reader.GetString(1)
                });
            }

            return products;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while searching Products: {ex.Message}");
            throw;
        }
    }

    private static DataTable CreateVariantTable(IEnumerable<ProductVariants> variants)
    {
        var table = new DataTable();

        // Must match the DB type column order exactly:
        table.Columns.Add("SKU", typeof(string));
        table.Columns.Add("Barcode", typeof(string));
        table.Columns.Add("Size", typeof(string));
        table.Columns.Add("Color", typeof(string));
        table.Columns.Add("Material", typeof(string));
        table.Columns.Add("Gender", typeof(string));
        table.Columns.Add("Season", typeof(string));
        table.Columns.Add("Status", typeof(bool));

        foreach (var item in variants)
        {
            table.Rows.Add(
                item.SKU ?? (object)DBNull.Value,
                item.Barcode ?? (object)DBNull.Value,
                item.Size ?? (object)DBNull.Value,
                item.Color ?? (object)DBNull.Value,
                item.Material ?? (object)DBNull.Value,
                item.Gender ?? (object)DBNull.Value,
                item.Season ?? (object)DBNull.Value,
                item.Status
            );
        }

        return table;
    }

    private static async Task<int> CreateWithinTransactionAsync(SqlConnection connection, SqlTransaction transaction, CreateProductRequest request)
    {
        using var command = new SqlCommand("usp_CreateProduct", connection, transaction) { CommandType = CommandType.StoredProcedure };
        command.Parameters.AddWithValue("@ProductCode", request.ProductMaster.ProductCode);
        command.Parameters.AddWithValue("@ProductName", request.ProductMaster.ProductName);
        command.Parameters.AddWithValue("@CategoryId", request.ProductMaster.CategoryId);
        command.Parameters.AddWithValue("@BrandId", (object?)request.ProductMaster.BrandId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Description", (object?)request.ProductMaster.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@HSNCode", (object?)request.ProductMaster.HSNCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@GSTPercentage", request.ProductMaster.GSTPercentage);
        command.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
        var variants = command.Parameters.AddWithValue("@Variants", CreateVariantTable(request.ProductVariants));
        variants.SqlDbType = SqlDbType.Structured;
        variants.TypeName = "dbo.ProductVariantType";
        var images = command.Parameters.AddWithValue("@Images", CreateImageTable(request.ProductImages));
        images.SqlDbType = SqlDbType.Structured;
        images.TypeName = "dbo.ProductImageType";
        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) throw new InvalidOperationException("Product creation did not return an identifier.");
        return Convert.ToInt32(reader["ProductMasterId"]);
    }

    private static async Task<bool> UpdateWithinTransactionAsync(SqlConnection connection, SqlTransaction transaction, int productMasterId, CreateProductRequest request)
    {
        using (var command = new SqlCommand(@"
UPDATE ProductMasters SET ProductCode=@ProductCode, ProductName=@ProductName, CategoryId=@CategoryId, BrandId=@BrandId,
Description=@Description, HSNCode=@HSNCode, GSTPercentage=@GSTPercentage, IsActive=@IsActive
WHERE ProductMasterId=@ProductMasterId;", connection, transaction))
        {
            command.Parameters.AddWithValue("@ProductMasterId", productMasterId);
            command.Parameters.AddWithValue("@ProductCode", request.ProductMaster.ProductCode);
            command.Parameters.AddWithValue("@ProductName", request.ProductMaster.ProductName);
            command.Parameters.AddWithValue("@CategoryId", request.ProductMaster.CategoryId);
            command.Parameters.AddWithValue("@BrandId", (object?)request.ProductMaster.BrandId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Description", (object?)request.ProductMaster.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@HSNCode", (object?)request.ProductMaster.HSNCode ?? DBNull.Value);
            command.Parameters.AddWithValue("@GSTPercentage", request.ProductMaster.GSTPercentage);
            command.Parameters.AddWithValue("@IsActive", request.ProductMaster.IsActive);
            if (await command.ExecuteNonQueryAsync() == 0) return false;
        }

        using (var command = new SqlCommand("UPDATE Products SET IsActive=@IsActive WHERE ProductMasterId=@ProductMasterId", connection, transaction))
        {
            command.Parameters.AddWithValue("@ProductMasterId", productMasterId);
            command.Parameters.AddWithValue("@IsActive", request.ProductMaster.IsActive);
            await command.ExecuteNonQueryAsync();
        }

        await ReplaceImagesAsync(connection, transaction, productMasterId, request.ProductImages);
        await ReplaceVariantsAsync(connection, transaction, productMasterId, request.ProductVariants);
        return true;
    }

    private static DataTable CreateImageTable(IEnumerable<ProductImages> images)
    {
        var table = new DataTable();

        table.Columns.Add("ImageUrl", typeof(string));
        table.Columns.Add("IsPrimary", typeof(bool));

        foreach (var item in images)
        {
            table.Rows.Add(
                item.ImageUrl ?? (object)DBNull.Value,
                item.IsPrimary
            );
        }

        return table;
    }

    private static ProductResponse MapProduct(SqlDataReader reader)
    {
        return new ProductResponse
        {
            ProductMasterId = reader.GetInt32(reader.GetOrdinal("ProductMasterId")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
            CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
            BrandId = reader.IsDBNull(reader.GetOrdinal("BrandId")) ? null : reader.GetInt32(reader.GetOrdinal("BrandId")),
            BrandName = reader.IsDBNull(reader.GetOrdinal("BrandName")) ? null : reader.GetString(reader.GetOrdinal("BrandName")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedDate"))
        };
    }

    private static async Task<List<ProductVariants>> GetVariantsAsync(
        SqlConnection connection,
        int productMasterId)
    {
        var variants = new List<ProductVariants>();

        using var command = new SqlCommand(@"
        SELECT
            pv.SKU,
            pv.Barcode,
            pv.Size,
            pv.Color,
            pv.Material,
            pv.Gender,
            pv.Season,
            pv.Status,
            pvp.CostPrice,
            pvp.SalePrice,
            pvp.EffectiveFrom,
            pvp.EffectiveTo,
            pvp.PlatformFee,
            pvp.Discount,
            pvp.DiscountType
        FROM ProductVariants pv
        LEFT JOIN ProductVariantPrice pvp
            ON pv.VariantId = pvp.VariantId
        WHERE pv.ProductMasterId = @ProductMasterId;",
            connection);

        command.Parameters.AddWithValue("@ProductMasterId", productMasterId);

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            variants.Add(new ProductVariants
            {
                SKU = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                Barcode = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Size = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Color = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Material = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                Gender = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Season = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                Status = !reader.IsDBNull(7) && reader.GetBoolean(7),

                ProductVariantPrice = new ProductVariantPrice
                {
                    CostPrice = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                    SalePrice = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9),
                    EffectiveFrom = reader.IsDBNull(10)
                        ? DateTime.MinValue
                        : reader.GetDateTime(10),
                    EffectiveTo = reader.IsDBNull(11)
                        ? null
                        : reader.GetDateTime(11),
                    PlatformFee = reader.IsDBNull(12)
                        ? 0
                        : reader.GetDecimal(12),
                    Discount = reader.IsDBNull(13)
                        ? 0
                        : reader.GetInt32(13),
                    DiscountType = reader.IsDBNull(14)
                        ? string.Empty
                        : reader.GetString(14)
                }
            });
        }

        return variants;
    }
    private static async Task<List<ProductImages>> GetImagesAsync(SqlConnection connection, int productMasterId)
    {
        var images = new List<ProductImages>();

        using var command = new SqlCommand(@"
SELECT ImageUrl, IsPrimary
FROM ProductImages
WHERE ProductMasterId = @ProductMasterId;", connection);

        command.Parameters.AddWithValue("@ProductMasterId", productMasterId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            images.Add(new ProductImages
            {
                ImageUrl = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                IsPrimary = reader.GetBoolean(1)
            });
        }

        return images;
    }

    private static async Task ReplaceImagesAsync(SqlConnection connection, SqlTransaction transaction, int productMasterId, IEnumerable<ProductImages> images)
    {
        var productId = await GetProductIdAsync(connection, transaction, productMasterId);
        if (productId == null) return;

        using (var deleteCommand = new SqlCommand("DELETE FROM ProductImages WHERE ProductMasterId = @ProductMasterId", connection, transaction))
        {
            deleteCommand.Parameters.AddWithValue("@ProductMasterId", productMasterId);
            await deleteCommand.ExecuteNonQueryAsync();
        }

        foreach (var image in images)
        {
            using var insertCommand = new SqlCommand(@"
INSERT INTO ProductImages (ProductMasterId, ProductId, ImageUrl, IsPrimary, CreatedDate)
VALUES (@ProductMasterId, @ProductId, @ImageUrl, @IsPrimary, GETDATE());", connection, transaction);

            insertCommand.Parameters.AddWithValue("@ProductMasterId", productMasterId);
            insertCommand.Parameters.AddWithValue("@ProductId", productId.Value);
            insertCommand.Parameters.AddWithValue("@ImageUrl", image.ImageUrl);
            insertCommand.Parameters.AddWithValue("@IsPrimary", image.IsPrimary);

            await insertCommand.ExecuteNonQueryAsync();
        }
    }

    private static async Task ReplaceVariantsAsync(SqlConnection connection, SqlTransaction transaction, int productMasterId, IEnumerable<ProductVariants> variants)
    {
        var productId = await GetProductIdAsync(connection, transaction, productMasterId);
        if (productId == null) return;

        var incoming = variants.ToList();
        foreach (var variant in incoming)
        {
            using var updateCommand = new SqlCommand(@"
UPDATE ProductVariants SET Barcode=@Barcode, Size=@Size, Color=@Color, Material=@Material,
Gender=@Gender, Season=@Season, CurrentPrice=@CurrentPrice, Status=@Status
WHERE ProductMasterId=@ProductMasterId AND SKU=@SKU;", connection, transaction);
            AddVariantParameters(updateCommand, productMasterId, variant);
            if (await updateCommand.ExecuteNonQueryAsync() > 0) continue;

            using var insertCommand = new SqlCommand(@"
INSERT INTO ProductVariants (ProductMasterId, ProductId, SKU, Barcode, Size, Color, Material, Gender, Season, CurrentPrice, Status)
VALUES (@ProductMasterId, @ProductId, @SKU, @Barcode, @Size, @Color, @Material, @Gender, @Season, @Status);", connection, transaction);
            insertCommand.Parameters.AddWithValue("@ProductId", productId.Value);
            AddVariantParameters(insertCommand, productMasterId, variant);
            await insertCommand.ExecuteNonQueryAsync();
        }

        using var deactivateCommand = new SqlCommand { Connection = connection, Transaction = transaction };
        deactivateCommand.Parameters.AddWithValue("@ProductMasterId", productMasterId);
        if (incoming.Count == 0)
        {
            deactivateCommand.CommandText = "UPDATE ProductVariants SET Status=0 WHERE ProductMasterId=@ProductMasterId";
        }
        else
        {
            var parameterNames = new List<string>(incoming.Count);
            for (var index = 0; index < incoming.Count; index++)
            {
                var name = $"@Sku{index}";
                parameterNames.Add(name);
                deactivateCommand.Parameters.AddWithValue(name, incoming[index].SKU);
            }
            deactivateCommand.CommandText = $"UPDATE ProductVariants SET Status=0 WHERE ProductMasterId=@ProductMasterId AND SKU NOT IN ({string.Join(',', parameterNames)})";
        }
        await deactivateCommand.ExecuteNonQueryAsync();
    }

    private static void AddVariantParameters(SqlCommand command, int productMasterId, ProductVariants variant)
    {
        command.Parameters.AddWithValue("@ProductMasterId", productMasterId);
        command.Parameters.AddWithValue("@SKU", variant.SKU);
        command.Parameters.AddWithValue("@Barcode", (object?)variant.Barcode ?? DBNull.Value);
        command.Parameters.AddWithValue("@Size", (object?)variant.Size ?? DBNull.Value);
        command.Parameters.AddWithValue("@Color", (object?)variant.Color ?? DBNull.Value);
        command.Parameters.AddWithValue("@Material", (object?)variant.Material ?? DBNull.Value);
        command.Parameters.AddWithValue("@Gender", (object?)variant.Gender ?? DBNull.Value);
        command.Parameters.AddWithValue("@Season", (object?)variant.Season ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", variant.Status);
    }

    private static void TryRollback(SqlTransaction transaction)
    {
        try { transaction.Rollback(); }
        catch (InvalidOperationException) { }
        catch (SqlException) { }
    }

    private static async Task<int?> GetProductIdAsync(SqlConnection connection, SqlTransaction transaction, int productMasterId)
    {
        using var command = new SqlCommand("SELECT ProductId FROM Products WHERE ProductMasterId = @ProductMasterId", connection, transaction);
        command.Parameters.AddWithValue("@ProductMasterId", productMasterId);

        var result = await command.ExecuteScalarAsync();
        return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }
}
