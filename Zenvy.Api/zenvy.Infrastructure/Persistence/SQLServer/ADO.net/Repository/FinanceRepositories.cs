using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Dashboard;
using zenvy.application.DTOs.Finance;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

internal static class FinanceSql
{
    public static SqlCommand Command(string name, SqlConnection connection) =>
        new(name, connection) { CommandType = CommandType.StoredProcedure };

    public static void AddNullable(this SqlParameterCollection parameters, string name, object? value) =>
        parameters.AddWithValue(name, value ?? DBNull.Value);

    public static void AddGuid(this SqlParameterCollection parameters, string name, string? value, bool nullable = false)
    {
        var parameter = parameters.Add(name, SqlDbType.UniqueIdentifier);
        if (string.IsNullOrWhiteSpace(value) && nullable)
            parameter.Value = DBNull.Value;
        else if (Guid.TryParse(value, out var id))
            parameter.Value = id;
        else
            throw new ArgumentException($"{name.TrimStart('@')} must be a valid GUID.", name);
    }
}

public class ExpenseRepository(IConfiguration configuration) : IExpenseRepository
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    public async Task<int> CreateTypeAsync(ExpenseTypeRequest request)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_CreateExpenseType", connection);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddNullable("@Description", request.Description);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<ExpenseTypeResponse>> GetTypesAsync()
    {
        var result = new List<ExpenseTypeResponse>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_GetExpenseTypes", connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new ExpenseTypeResponse
        {
            ExpenseTypeId = reader.GetInt32(reader.GetOrdinal("ExpenseTypeId")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
        });
        return result;
    }

    public async Task<long> CreateExpenseAsync(ExpenseRequest request)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_CreateExpense", connection);
        command.Parameters.AddWithValue("@ExpenseTypeId", request.ExpenseTypeId);
        
        command.Parameters.AddWithValue("@Amount", request.Amount);
        command.Parameters.AddNullable("@Description", request.Description);
        command.Parameters.AddWithValue("@ExpenseDate", request.ExpenseDate);
        command.Parameters.AddGuid("@CreatedBy", request.CreatedBy);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<ExpenseResponse>> GetExpensesAsync(DateTime? fromDate, DateTime? toDate, int? expenseTypeId)
    {
        var result = new List<ExpenseResponse>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_GetExpenses", connection);
        command.Parameters.AddNullable("@FromDate", fromDate);
        command.Parameters.AddNullable("@ToDate", toDate);
        command.Parameters.AddNullable("@ExpenseTypeId", expenseTypeId);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new ExpenseResponse
        {
            ExpenseId = reader.GetInt64(reader.GetOrdinal("ExpenseId")),
            ExpenseTypeId = reader.GetInt32(reader.GetOrdinal("ExpenseTypeId")),
            ExpenseTypeName = reader.GetString(reader.GetOrdinal("ExpenseTypeName")),
            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
            CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")).ToString(),
            CreatedByName = reader.GetString(reader.GetOrdinal("CreatedByName"))
        });
        return result;
    }
}

public class ProfitRepository(IConfiguration configuration) : IProfitRepository
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    public async Task<ProfitSourceData> GetSourceDataAsync(DateTime fromDate, DateTime toDate)
    {
        var result = new ProfitSourceData();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_GetProfitSourceData", connection);
        command.Parameters.AddWithValue("@FromDate", fromDate);
        command.Parameters.AddWithValue("@ToDate", toDate);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Purchases.Add(new PurchaseCostSource(reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2)));
        await reader.NextResultAsync();
        while (await reader.ReadAsync()) result.Sales.Add(new SaleSource(reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2)));
        await reader.NextResultAsync();
        while (await reader.ReadAsync()) result.Returns.Add(new ReturnSource(reader.GetInt32(0), reader.GetInt32(1), reader.GetDecimal(2)));
        await reader.NextResultAsync();
        while (await reader.ReadAsync()) result.Expenses.Add(new AmountSource(reader.GetDecimal(0)));
        await reader.NextResultAsync();
        while (await reader.ReadAsync()) result.Commissions.Add(new AmountSource(reader.GetDecimal(0)));
        return result;
    }
}

public class EmployeeCommissionRepository(IConfiguration configuration) : IEmployeeCommissionRepository
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    public async Task<long> CreateAsync(EmployeeCommissionRequest request)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_CreateEmployeeCommission", connection);
        command.Parameters.AddGuid("@UserId", request.UserId);
        command.Parameters.AddWithValue("@OrderId", request.OrderId);
        command.Parameters.AddWithValue("@CommissionPercent", request.CommissionPercent);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<EmployeeCommissionResponse>> GetAllAsync(string? userId, DateTime? fromDate, DateTime? toDate)
    {
        var result = new List<EmployeeCommissionResponse>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_GetEmployeeCommissions", connection);
        command.Parameters.AddGuid("@UserId", userId, nullable: true); command.Parameters.AddNullable("@FromDate", fromDate); command.Parameters.AddNullable("@ToDate", toDate);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new EmployeeCommissionResponse
        {
            CommissionId = reader.GetInt64(reader.GetOrdinal("CommissionId")), UserId = reader.GetString(reader.GetOrdinal("UserId")).ToString(),
            EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")), OrderId = reader.GetInt64(reader.GetOrdinal("OrderId")),
            OrderAmount = reader.GetDecimal(reader.GetOrdinal("OrderAmount")), CommissionPercent = reader.GetDecimal(reader.GetOrdinal("CommissionPercent")),
            CommissionAmount = reader.GetDecimal(reader.GetOrdinal("CommissionAmount")), CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        });
        return result;
    }
}

public class InvestorRepository(IConfiguration configuration) : IInvestorRepository
{
    private readonly string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    public async Task<int> CreateAsync(InvestorRequest request)
    {
        await using var connection = new SqlConnection(connectionString); await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_CreateInvestor", connection);
        command.Parameters.AddWithValue("@Name", request.Name); command.Parameters.AddNullable("@Email", request.Email); command.Parameters.AddNullable("@Phone", request.Phone);
        command.Parameters.AddWithValue("@InvestmentAmount", request.InvestmentAmount); command.Parameters.AddWithValue("@OwnershipPercent", request.OwnershipPercent); 
        command.Parameters.AddWithValue("@LossPercent", request.LossPercent); 
        command.Parameters.AddWithValue("@JoinDate", request.JoinDate);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<InvestorResponse>> GetAllAsync()
    {
        var result = new List<InvestorResponse>();
        await using var connection = new SqlConnection(connectionString); await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_GetInvestors", connection); await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new InvestorResponse
        {
            InvestorId = reader.GetInt32(reader.GetOrdinal("InvestorId")), Name = reader.GetString(reader.GetOrdinal("Name")),
            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")), Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
            InvestmentAmount = reader.GetDecimal(reader.GetOrdinal("InvestmentAmount")), OwnershipPercent = reader.GetDecimal(reader.GetOrdinal("OwnershipPercent")),LossPercent = reader.GetDecimal(reader.GetOrdinal("LossPercent")),
            JoinDate = reader.GetDateTime(reader.GetOrdinal("JoinDate")), Status = reader.GetBoolean(reader.GetOrdinal("Status"))
        });
        return result;
    }

    public async Task<int> DistributeProfitAsync(ProfitDistributionRequest request, decimal netProfit)
    {
        await using var connection = new SqlConnection(connectionString); await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_DistributeInvestorProfit", connection);
        command.Parameters.AddWithValue("@Month", request.Month); command.Parameters.AddWithValue("@Year", request.Year);
        command.Parameters.AddWithValue("@NetProfit", netProfit);
        command.Parameters.AddNullable("@DistributedDate", request.DistributedDate); command.Parameters.AddNullable("@Notes", request.Notes);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<IEnumerable<ProfitDistributionResponse>> GetDistributionsAsync(short? year, byte? month, int? investorId)
    {
        var result = new List<ProfitDistributionResponse>();
        await using var connection = new SqlConnection(connectionString); await connection.OpenAsync();
        await using var command = FinanceSql.Command("usp_GetProfitDistributions", connection);
        command.Parameters.AddNullable("@Year", year); command.Parameters.AddNullable("@Month", month); command.Parameters.AddNullable("@InvestorId", investorId);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(new ProfitDistributionResponse
        {
            DistributionId = reader.GetInt64(reader.GetOrdinal("DistributionId")), InvestorId = reader.GetInt32(reader.GetOrdinal("InvestorId")), InvestorName = reader.GetString(reader.GetOrdinal("InvestorName")),
            Month = reader.GetByte(reader.GetOrdinal("Month")), Year = reader.GetInt16(reader.GetOrdinal("Year")), OwnershipPercent = reader.GetDecimal(reader.GetOrdinal("OwnershipPercent")),
            ProfitAmount = reader.GetDecimal(reader.GetOrdinal("ProfitAmount")), DistributedDate = reader.IsDBNull(reader.GetOrdinal("DistributedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("DistributedDate")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes"))
        });
        return result;
    }
}
