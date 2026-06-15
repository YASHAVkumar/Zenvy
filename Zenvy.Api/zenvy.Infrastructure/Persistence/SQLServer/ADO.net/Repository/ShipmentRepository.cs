using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Shipments;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class ShipmentRepository(IConfiguration configuration) : IShipmentRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<long> CreateAsync(ShipmentRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreateShipment", connection);
        command.Parameters.AddWithValue("@OrderId", request.OrderId);
        command.Parameters.AddWithValue("@CourierName", (object?)request.CourierName ?? DBNull.Value);
        command.Parameters.AddWithValue("@TrackingNumber", (object?)request.TrackingNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("@ShippedDate", (object?)request.ShippedDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@DeliveredDate", (object?)request.DeliveredDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", (object?)request.Status ?? DBNull.Value);

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<ShipmentResponse>> GetAllAsync()
    {
        var shipments = new List<ShipmentResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetShipments", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            shipments.Add(new ShipmentResponse
            {
                ShipmentId = reader.GetInt64(reader.GetOrdinal("ShipmentId")),
                OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
                CourierName = reader.IsDBNull(reader.GetOrdinal("CourierName")) ? null : reader.GetString(reader.GetOrdinal("CourierName")),
                TrackingNumber = reader.IsDBNull(reader.GetOrdinal("TrackingNumber")) ? null : reader.GetString(reader.GetOrdinal("TrackingNumber")),
                ShippedDate = reader.IsDBNull(reader.GetOrdinal("ShippedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ShippedDate")),
                DeliveredDate = reader.IsDBNull(reader.GetOrdinal("DeliveredDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DeliveredDate")),
                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status"))
            });
        }

        return shipments;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }
}
