using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Suppliers;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class SupplierRepository(IConfiguration configuration) : ISupplierRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<int> CreateAsync(SupplierRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreateSupplier", connection);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@ContactPerson", (object?)request.ContactPerson ?? DBNull.Value);
        command.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@AddressLine1", (object?)request?.Address?.AddressLine1 ?? DBNull.Value);
        command.Parameters.AddWithValue("@AddressLine2", (object?)request?.Address?.AddressLine2 ?? DBNull.Value);
        command.Parameters.AddWithValue("@City", (object?)request?.Address?.City ?? DBNull.Value);
        command.Parameters.AddWithValue("@Country", (object?)request?.Address?.Country ?? DBNull.Value);
        command.Parameters.AddWithValue("@PostalCode", (object?)request?.Address?.PostalCode ?? DBNull.Value);    
        command.Parameters.AddWithValue("@Status", request.Status);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<SupplierResponse>> GetAllAsync()
    {
        var suppliers = new List<SupplierResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetSuppliers", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            suppliers.Add(new SupplierResponse
            {
                SupplierId = reader.GetInt32(reader.GetOrdinal("SupplierId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                ContactPerson = GetNullableString(reader, "ContactPerson"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return suppliers;
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }

    private static string? GetNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
