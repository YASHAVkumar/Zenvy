using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Customers;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class CustomerRepository(IConfiguration configuration) : ICustomerRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<int> CreateAsync(CustomerRequest request)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_CreateCustomer", connection);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Phone", (object?)request.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue("@Email", (object?)request.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("@Address1", (object?)request.Address1 ?? DBNull.Value);
        command.Parameters.AddWithValue("@Address2", (object?)request.Address2 ?? DBNull.Value);
        command.Parameters.AddWithValue("@City", (object?)request.City ?? DBNull.Value);
        command.Parameters.AddWithValue("@State", (object?)request.State ?? DBNull.Value);
        command.Parameters.AddWithValue("@Pincode", (object?)request.Pincode ?? DBNull.Value);
        command.Parameters.AddWithValue("@Country", (object?)request.Country ?? DBNull.Value);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllAsync()
    {
        var customers = new List<CustomerResponse>();

        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetCustomers", connection);
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(new CustomerResponse
            {
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address1 = GetNullableString(reader, "Address1"),
                Address2 = GetNullableString(reader, "Address2"),
                City = GetNullableString(reader, "City"),
                State = GetNullableString(reader, "State"),
                Pincode = GetNullableString(reader, "Pincode"),
                Country = GetNullableString(reader, "Country"),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return customers;
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
