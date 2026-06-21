using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Inventory;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class InventoryRepository(IConfiguration configuration) : IInventoryRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<int> CreateAsync(InventoryRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreateInventory", connection);
        AddInventoryParameters(command, request);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<InventoryResponse>> GetAllAsync()
    {
        var inventory = new List<InventoryResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetInventory", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            inventory.Add(MapInventory(reader));
        }

        return inventory;
    }

    public async Task<InventoryResponse?> GetByIdAsync(int inventoryId)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetInventoryById", connection);
        command.Parameters.AddWithValue("@InventoryId", inventoryId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapInventory(reader);
        }

        return null;
    }

    public async Task<bool> UpdateAsync(int inventoryId, InventoryRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_UpdateInventory", connection);
        command.Parameters.AddWithValue("@InventoryId", inventoryId);
        AddInventoryParameters(command, request);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> AdjustAsync(InventoryAdjustmentRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_AdjustInventory", connection);
        command.Parameters.AddWithValue("@VariantId", request.VariantId);
        command.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
        command.Parameters.AddWithValue("@Quantity", request.Quantity);
        command.Parameters.AddWithValue("@ReferenceType", (object?)request.ReferenceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReferenceId", (object?)request.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", Guid.TryParse(request.CreatedBy, out var createdBy) ? createdBy : DBNull.Value);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DamageAsync(InventoryDamageRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_DamageInventory", connection);
        command.Parameters.AddWithValue("@VariantId", request.VariantId);
        command.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
        command.Parameters.AddWithValue("@Quantity", request.Quantity);
        command.Parameters.AddWithValue("@ReferenceType", (object?)request.ReferenceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReferenceId", (object?)request.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", Guid.TryParse(request.CreatedBy, out var createdBy) ? createdBy : DBNull.Value);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> TransferAsync(InventoryTransferRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_TransferInventory", connection);
        command.Parameters.AddWithValue("@VariantId", request.VariantId);
        command.Parameters.AddWithValue("@FromWarehouseId", request.FromWarehouseId);
        command.Parameters.AddWithValue("@ToWarehouseId", request.ToWarehouseId);
        command.Parameters.AddWithValue("@Quantity", request.Quantity);
        command.Parameters.AddWithValue("@ReferenceType", (object?)request.ReferenceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReferenceId", (object?)request.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("@CreatedBy", Guid.TryParse(request.CreatedBy, out var createdBy) ? createdBy : DBNull.Value);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public Task<IEnumerable<InventoryTransactionResponse>> GetTransactionsAsync()
    {
        return GetTransactionsInternalAsync(null);
    }

    public Task<IEnumerable<InventoryTransactionResponse>> GetTransactionsByVariantAsync(int variantId)
    {
        return GetTransactionsInternalAsync(variantId);
    }

    private async Task<IEnumerable<InventoryTransactionResponse>> GetTransactionsInternalAsync(int? variantId)
    {
        var transactions = new List<InventoryTransactionResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetInventoryTransactions", connection);
        command.Parameters.AddWithValue("@VariantId", (object?)variantId ?? DBNull.Value);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            transactions.Add(MapTransaction(reader));
        }

        return transactions;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
    }

    private static void AddInventoryParameters(SqlCommand command, InventoryRequest request)
    {
        command.Parameters.AddWithValue("@VariantId", request.VariantId);
        command.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
        command.Parameters.AddWithValue("@OnHandQty", request.OnHandQty);
        command.Parameters.AddWithValue("@ReservedQty", request.ReservedQty);
    }

    private static InventoryResponse MapInventory(SqlDataReader reader)
    {
        return new InventoryResponse
        {
            InventoryId = reader.GetInt32(reader.GetOrdinal("InventoryId")),
            ProductMasterId = reader.GetInt32(reader.GetOrdinal("ProductMasterId")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            VariantId = reader.GetInt32(reader.GetOrdinal("VariantId")),
            SKU = reader.GetString(reader.GetOrdinal("SKU")),
            WarehouseId = reader.GetInt32(reader.GetOrdinal("WarehouseId")),
            Warehouse = reader.GetString(reader.GetOrdinal("Warehouse")),
            OnHandQty = reader.GetInt32(reader.GetOrdinal("OnHandQty")),
            ReservedQty = reader.GetInt32(reader.GetOrdinal("ReservedQty")),
            AvailableQty = reader.GetInt32(reader.GetOrdinal("AvailableQty"))
        };
    }

    private static InventoryTransactionResponse MapTransaction(SqlDataReader reader)
    {
        return new InventoryTransactionResponse
        {
            TransactionId = reader.GetInt64(reader.GetOrdinal("TransactionId")),
            ProductMasterId = reader.GetInt32(reader.GetOrdinal("ProductMasterId")),
            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
            VariantId = reader.GetInt32(reader.GetOrdinal("VariantId")),
            SKU = reader.GetString(reader.GetOrdinal("SKU")),
            WarehouseId = reader.GetInt32(reader.GetOrdinal("WarehouseId")),
            Warehouse = reader.GetString(reader.GetOrdinal("Warehouse")),
            TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
            ReferenceType = reader.IsDBNull(reader.GetOrdinal("ReferenceType")) ? null : reader.GetString(reader.GetOrdinal("ReferenceType")),
            ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId")) ? null : reader.GetInt64(reader.GetOrdinal("ReferenceId")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetGuid(reader.GetOrdinal("CreatedBy")).ToString(),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
