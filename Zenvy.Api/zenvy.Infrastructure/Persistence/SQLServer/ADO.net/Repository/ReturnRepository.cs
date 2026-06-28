using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Returns;
using zenvy.application.Interfaces.Repositories;
using zenvy.Domain.Enums;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class ReturnRepository(IConfiguration configuration) : IReturnRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<long> CreateAsync(ReturnRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreateSalesReturn", connection);
        command.Parameters.AddWithValue("@OrderId", request.OrderId);
        command.Parameters.AddWithValue("@ReturnDate", request.ReturnDate);
        command.Parameters.AddWithValue("@Reason", (object?)request.Reason?.ToString() ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", EnumMappings.GetReturnStatusValue(request.Status));
        command.Parameters.AddWithValue("@RefundStatus", EnumMappings.GetRefundStatusValue(request.RefundStatus));
        command.Parameters.AddWithValue("@RefundMethod", (object?)request.RefundMethod ?? DBNull.Value);
        command.Parameters.AddWithValue("@ReturnShippingFee", request.ReturnShippingFee);
        command.Parameters.AddWithValue("@MarketplaceFee", request.MarketplaceFee);
        command.Parameters.AddWithValue("@DeliveryFeeRefunded", request.DeliveryFeeRefunded);
        command.Parameters.AddWithValue("@CreatedBy", Guid.TryParse(request.CreatedBy, out var createdBy) ? createdBy : DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)request.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@LinesJson", JsonSerializer.Serialize(request.Lines));

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<ReturnResponse>> GetAllAsync()
    {
        var returns = new List<ReturnResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetSalesReturns", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            returns.Add(MapHeader(reader));
        }

        return returns;
    }

    public async Task<ReturnResponse?> GetByIdAsync(long returnId)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetSalesReturnById", connection);
        command.Parameters.AddWithValue("@ReturnId", returnId);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var salesReturn = MapHeader(reader);
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            salesReturn.Lines.Add(new ReturnLineResponse
            {
                ReturnLineId = reader.GetInt64(reader.GetOrdinal("ReturnLineId")),
                ReturnId = reader.GetInt64(reader.GetOrdinal("ReturnId")),
                OrderLineId = reader.GetInt64(reader.GetOrdinal("OrderLineId")),
                ProductMasterId = reader.GetInt32(reader.GetOrdinal("ProductMasterId")),
                VariantId = reader.GetInt32(reader.GetOrdinal("VariantId")),
                SKU = reader.GetString(reader.GetOrdinal("SKU")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
                RefundAmount = reader.GetDecimal(reader.GetOrdinal("RefundAmount")),
                Condition = reader.IsDBNull(reader.GetOrdinal("Condition")) ? null : reader.GetString(reader.GetOrdinal("Condition")),
                Restock = reader.GetBoolean(reader.GetOrdinal("Restock")),
                RestockWarehouseId = reader.IsDBNull(reader.GetOrdinal("RestockWarehouseId")) ? null : reader.GetInt32(reader.GetOrdinal("RestockWarehouseId")),
                RestockWarehouseName = reader.IsDBNull(reader.GetOrdinal("RestockWarehouseName")) ? null : reader.GetString(reader.GetOrdinal("RestockWarehouseName"))
            });
        }

        return salesReturn;
    }

    public async Task<bool> UpdateStatusAsync(long returnId, ReturnStatusRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_UpdateSalesReturnStatus", connection);
        command.Parameters.AddWithValue("@ReturnId", returnId);
        command.Parameters.AddWithValue("@Status", request.Status);
        command.Parameters.AddWithValue("@RefundStatus", (object?)request.RefundStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@Notes", (object?)request.Notes ?? DBNull.Value);

        return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }

    private static ReturnResponse MapHeader(SqlDataReader reader)
    {
        return new ReturnResponse
        {
            ReturnId = reader.GetInt64(reader.GetOrdinal("ReturnId")),
            OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
            ChannelId = reader.GetInt32(reader.GetOrdinal("ChannelId")),
            ChannelName = reader.GetString(reader.GetOrdinal("ChannelName")),
            ExternalOrderId = reader.IsDBNull(reader.GetOrdinal("ExternalOrderId")) ? null : reader.GetString(reader.GetOrdinal("ExternalOrderId")),
            CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerId")),
            CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
            ReturnDate = reader.GetDateTime(reader.GetOrdinal("ReturnDate")),
            Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            RefundStatus = reader.GetString(reader.GetOrdinal("RefundStatus")),
            RefundMethod = reader.IsDBNull(reader.GetOrdinal("RefundMethod")) ? null : reader.GetString(reader.GetOrdinal("RefundMethod")),
            RefundAmount = reader.GetDecimal(reader.GetOrdinal("RefundAmount")),
            ReturnShippingFee = reader.GetDecimal(reader.GetOrdinal("ReturnShippingFee")),
            MarketplaceFee = reader.GetDecimal(reader.GetOrdinal("MarketplaceFee")),
            DeliveryFeeRefunded = reader.GetBoolean(reader.GetOrdinal("DeliveryFeeRefunded")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")).ToString(),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
