using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Payments;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class PaymentRepository(IConfiguration configuration) : IPaymentRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<long> CreateAsync(PaymentRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreatePayment", connection);
        command.Parameters.AddWithValue("@OrderId", request.OrderId);
        command.Parameters.AddWithValue("@PaymentMethodId", request.PaymentMethodId);
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddWithValue("@TransactionRef", (object?)request.TransactionRef ?? DBNull.Value);
        command.Parameters.AddWithValue("@Status", (object?)request.Status ?? DBNull.Value);
        command.Parameters.AddWithValue("@PaymentDate", request.PaymentDate);

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllAsync()
    {
        var payments = new List<PaymentResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetPayments", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(new PaymentResponse
            {
                PaymentId = reader.GetInt64(reader.GetOrdinal("PaymentId")),
                OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
                PaymentMethodId = reader.GetInt32(reader.GetOrdinal("PaymentMethodId")),
                MethodName = reader.GetString(reader.GetOrdinal("MethodName")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                TransactionRef = reader.IsDBNull(reader.GetOrdinal("TransactionRef")) ? null : reader.GetString(reader.GetOrdinal("TransactionRef")),
                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate"))
            });
        }

        return payments;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }
}
