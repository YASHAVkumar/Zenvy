using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.SalesOrders;
using zenvy.application.Interfaces.Repositories;
using zenvy.Domain.Enums;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class SalesOrderRepository(IConfiguration configuration) : ISalesOrderRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<long> CreateAsync(SalesOrderRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreateSalesOrder", connection);
        command.Parameters.AddWithValue("@CustomerId", (object?)request.CustomerId ?? DBNull.Value);
        command.Parameters.AddWithValue("@ChannelId", request.ChannelId);
        command.Parameters.AddWithValue("@CreatedBy", Guid.Parse(request.CreatedBy));
        command.Parameters.AddWithValue("@ExternalOrderId", (object?)request.ExternalOrderId ?? DBNull.Value);
        command.Parameters.AddWithValue("@OrderDate", request.OrderDate);
        command.Parameters.AddWithValue("@Status", EnumMappings.GetOrderStatusValue(request.Status));
        command.Parameters.AddWithValue("@ShippingFee", request.ShippingFee);
        command.Parameters.AddWithValue("@LinesJson", JsonSerializer.Serialize(request.Lines));
        command.Parameters.AddWithValue("@PaymentMethodId", request.PaymentMethodId);
        command.Parameters.AddWithValue("@ReferenceId", (object?)request.ReferenceId ?? DBNull.Value);
        command.Parameters.AddWithValue("@PayStatus", EnumMappings.GetPaymentStatusValue(request.PayStatus));
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<SalesOrderResponse>> GetAllAsync()
    {
        var orders = new List<SalesOrderResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetSalesOrders", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            orders.Add(MapHeader(reader));
        }

        return orders;
    }

    public async Task<SalesOrderResponse?> GetByIdAsync(long orderId)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetSalesOrderById", connection);
        command.Parameters.AddWithValue("@OrderId", orderId);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var order = MapHeader(reader);
        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            order.Lines.Add(new SalesOrderLineResponse
            {
                OrderLineId = reader.GetInt64(reader.GetOrdinal("OrderLineId")),
                OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
                ProductMasterId = reader.GetInt32(reader.GetOrdinal("ProductMasterId")),
                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                VariantId = reader.GetInt32(reader.GetOrdinal("VariantId")),
                SKU = reader.GetString(reader.GetOrdinal("SKU")),
                Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
                UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                Discount = reader.GetDecimal(reader.GetOrdinal("Discount")),
                Tax = reader.GetDecimal(reader.GetOrdinal("Tax")),
                LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
            });
        }

        return order;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }

    private static SalesOrderResponse MapHeader(SqlDataReader reader)
    {
        return new SalesOrderResponse
        {
            OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
            CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerId")),
            CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
            ChannelId = reader.GetInt32(reader.GetOrdinal("ChannelId")),
            ChannelName = reader.GetString(reader.GetOrdinal("ChannelName")),
            CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")).ToString(),
            ExternalOrderId = reader.IsDBNull(reader.GetOrdinal("ExternalOrderId")) ? null : reader.GetString(reader.GetOrdinal("ExternalOrderId")),
            OrderDate = reader.GetDateTime(reader.GetOrdinal("OrderDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
            Discount = reader.GetDecimal(reader.GetOrdinal("Discount")),
            Tax = reader.GetDecimal(reader.GetOrdinal("Tax")),
            ShippingFee = reader.GetDecimal(reader.GetOrdinal("ShippingFee")),
            GrandTotal = reader.GetDecimal(reader.GetOrdinal("GrandTotal")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}
