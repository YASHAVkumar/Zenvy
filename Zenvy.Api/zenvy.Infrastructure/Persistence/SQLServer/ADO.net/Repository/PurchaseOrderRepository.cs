using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.PurchaseOrders;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class PurchaseOrderRepository(IConfiguration configuration) : IPurchaseOrderRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<long> CreateAsync(PurchaseOrderRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreatePurchaseOrder", connection);
        command.Parameters.AddWithValue("@SupplierId", request.SupplierId);
        command.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
        command.Parameters.AddWithValue("@PONumber", request.PONumber);
        command.Parameters.AddWithValue("@OrderDate", request.OrderDate);
        command.Parameters.AddWithValue("@ExpectedDate", (object?)request.ExpectedDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", request.Status);
        command.Parameters.AddWithValue("@CreatedBy", request.CreatedBy);
        command.Parameters.AddWithValue("@LinesJson", JsonSerializer.Serialize(request.Lines));

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync()
    {
        var orders = new List<PurchaseOrderResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetPurchaseOrders", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(MapHeader(reader));
        }

        return orders;
    }

    public async Task<PurchaseOrderResponse?> GetByIdAsync(long poId)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetPurchaseOrderById", connection);
        command.Parameters.AddWithValue("@POId", poId);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var order = MapHeader(reader);
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            order.Lines.Add(new PurchaseOrderLineResponse
            {
                POLineId = reader.GetInt64(reader.GetOrdinal("POLineId")),
                POId = reader.GetInt64(reader.GetOrdinal("POId")),
                ProductMasterId = reader.GetInt32(reader.GetOrdinal("ProductMasterId")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                VariantId = reader.GetInt32(reader.GetOrdinal("VariantId")),
                SKU = reader.GetString(reader.GetOrdinal("SKU")),
                Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
                UnitCost = reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
                LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
            });
        }

        return order;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }

    private static PurchaseOrderResponse MapHeader(SqlDataReader reader)
    {
        return new PurchaseOrderResponse
        {
            POId = reader.GetInt64(reader.GetOrdinal("POId")),
            SupplierId = reader.GetInt32(reader.GetOrdinal("SupplierId")),
            SupplierName = reader.GetString(reader.GetOrdinal("SupplierName")),
            WarehouseId = reader.GetInt32(reader.GetOrdinal("WarehouseId")),
            WarehouseName = reader.GetString(reader.GetOrdinal("WarehouseName")),
            PONumber = reader.GetString(reader.GetOrdinal("PONumber")),
            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
            ExpectedDate = reader.IsDBNull(reader.GetOrdinal("ExpectedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpectedDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
            TaxAmount = reader.GetDecimal(reader.GetOrdinal("TaxAmount")),
            GrandTotal = reader.GetDecimal(reader.GetOrdinal("GrandTotal")),
            CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
