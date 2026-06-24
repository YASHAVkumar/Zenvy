using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using zenvy.application.DTOs.Dashboards;
using zenvy.application.Interfaces.Repositories;

namespace zenvy.infrastructure.persistence.sqlserver.ado.net.repository;

public class DashboardRepository(IConfiguration configuration) : IDashboardRepository
{
    private readonly string sqlConnectionString = configuration?.GetConnectionString("DefaultConnection") ?? "";

    public async Task<ExecutiveDashboardResponse?> GetExecutiveDashboardAsync()
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetExecutiveDashboard", connection);
        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var kpiCards = new ExecutiveDashboardKpiCards
        {
            TodaysSales = reader.GetDecimal(reader.GetOrdinal("TodaysSales")),
            MonthlySales = reader.GetDecimal(reader.GetOrdinal("MonthlySales")),
            MonthlyProfit = reader.GetDecimal(reader.GetOrdinal("MonthlyProfit")),
            InventoryValue = reader.GetDecimal(reader.GetOrdinal("InventoryValue")),
            AvailableCash = reader.GetDecimal(reader.GetOrdinal("AvailableCash")),
            ReserveFund = reader.GetDecimal(reader.GetOrdinal("ReserveFund")),
            InvestorLiability = reader.GetDecimal(reader.GetOrdinal("InvestorLiability")),
            PendingOrders = reader.GetInt32(reader.GetOrdinal("PendingOrders")),
            PendingReturns = reader.GetInt32(reader.GetOrdinal("PendingReturns"))
        };

        return new ExecutiveDashboardResponse
        {
            KpiCards = kpiCards,
            Charts = new ExecutiveDashboardCharts()
        };
    }

    public async Task<FinanceDashboardResponse?> GetFinanceDashboardAsync()
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetFinanceDashboard", connection);
        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var kpiCards = new FinanceDashboardKpiCards
        {
            CashAvailable = reader.GetDecimal(reader.GetOrdinal("CashAvailable")),
            BankBalance = reader.GetDecimal(reader.GetOrdinal("BankBalance")),
            WorkingCapital = reader.GetDecimal(reader.GetOrdinal("WorkingCapital")),
            ReserveFund = reader.GetDecimal(reader.GetOrdinal("ReserveFund")),
            InvestorCapital = reader.GetDecimal(reader.GetOrdinal("InvestorCapital")),
            OutstandingLoans = reader.GetDecimal(reader.GetOrdinal("OutstandingLoans")),
            Receivables = reader.GetDecimal(reader.GetOrdinal("Receivables")),
            Payables = reader.GetDecimal(reader.GetOrdinal("Payables"))
        };

        return new FinanceDashboardResponse
        {
            KpiCards = kpiCards,
            Reports = new FinanceDashboardReports()
        };
    }

    public async Task<OperationsDashboardResponse?> GetOperationsDashboardAsync()
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetOperationsDashboard", connection);
        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var kpiCards = new OperationsDashboardKpiCards
        {
            PendingPurchases = reader.GetInt32(reader.GetOrdinal("PendingPurchases")),
            PendingGoodsReceipt = reader.GetInt32(reader.GetOrdinal("PendingGoodsReceipt")),
            CurrentInventory = reader.GetDecimal(reader.GetOrdinal("CurrentInventory")),
            PendingOrders = reader.GetInt32(reader.GetOrdinal("PendingOrders")),
            PendingDispatch = reader.GetInt32(reader.GetOrdinal("PendingDispatch")),
            Returns = reader.GetInt32(reader.GetOrdinal("Returns")),
            DamagedStock = reader.GetInt32(reader.GetOrdinal("DamagedStock"))
        };

        return new OperationsDashboardResponse
        {
            KpiCards = kpiCards
        };
    }

    public async Task<MarketplaceDashboardResponse?> GetMarketplaceDashboardAsync()
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetMarketplaceDashboard", connection);
        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var kpiCards = new MarketplaceDashboardKpiCards
        {
            TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
            DeliveredOrders = reader.GetInt32(reader.GetOrdinal("DeliveredOrders")),
            CancelledOrders = reader.GetInt32(reader.GetOrdinal("CancelledOrders")),
            ReturnedOrders = reader.GetInt32(reader.GetOrdinal("ReturnedOrders")),
            SettlementPending = reader.GetInt32(reader.GetOrdinal("SettlementPending")),
            SettlementReceived = reader.GetDecimal(reader.GetOrdinal("SettlementReceived"))
        };

        return new MarketplaceDashboardResponse
        {
            KpiCards = kpiCards
        };
    }

    public async Task<InvestorDashboardResponse?> GetInvestorDashboardAsync(int investorId)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetInvestorDashboard", connection);
        command.Parameters.AddWithValue("@InvestorId", investorId);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var kpiCards = new InvestorDashboardKpiCards
        {
            TotalInvested = reader.GetDecimal(reader.GetOrdinal("TotalInvested")),
            CurrentCapital = reader.GetDecimal(reader.GetOrdinal("CurrentCapital")),
            ProfitEarned = reader.GetDecimal(reader.GetOrdinal("ProfitEarned")),
            ProfitPaid = reader.GetDecimal(reader.GetOrdinal("ProfitPaid")),
            PendingProfit = reader.GetDecimal(reader.GetOrdinal("PendingProfit")),
            RoiPercent = reader.GetDecimal(reader.GetOrdinal("RoiPercent"))
        };

        return new InvestorDashboardResponse
        {
            KpiCards = kpiCards
        };
    }

    public async Task<EmployeeDashboardResponse?> GetEmployeeDashboardAsync(int? employeeId = null)
    {
        using var connection = new SqlConnection(sqlConnectionString);
        await connection.OpenAsync();

        using var command = CreateStoredProcedureCommand("usp_GetEmployeeDashboard", connection);
        if (employeeId.HasValue)
            command.Parameters.AddWithValue("@EmployeeId", employeeId.Value);
        else
            command.Parameters.AddWithValue("@EmployeeId", DBNull.Value);

        using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var kpiCards = new EmployeeDashboardKpiCards
        {
            TasksAssigned = reader.GetInt32(reader.GetOrdinal("TasksAssigned")),
            TasksCompleted = reader.GetInt32(reader.GetOrdinal("TasksCompleted")),
            AttendanceDays = reader.GetInt32(reader.GetOrdinal("AttendanceDays")),
            Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
            Commission = reader.GetDecimal(reader.GetOrdinal("Commission")),
            PerformanceScore = reader.GetDecimal(reader.GetOrdinal("PerformanceScore"))
        };

        return new EmployeeDashboardResponse
        {
            KpiCards = kpiCards
        };
    }

    private static SqlCommand CreateStoredProcedureCommand(string procedureName, SqlConnection connection)
    {
        return new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure };
    }
}
