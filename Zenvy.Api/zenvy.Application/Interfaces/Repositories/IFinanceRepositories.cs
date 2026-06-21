using zenvy.application.DTOs.Dashboard;
using zenvy.application.DTOs.Finance;

namespace zenvy.application.Interfaces.Repositories;

public interface IExpenseRepository
{
    Task<int> CreateTypeAsync(ExpenseTypeRequest request);
    Task<IEnumerable<ExpenseTypeResponse>> GetTypesAsync();
    Task<long> CreateExpenseAsync(ExpenseRequest request);
    Task<IEnumerable<ExpenseResponse>> GetExpensesAsync(DateTime? fromDate, DateTime? toDate, int? expenseTypeId);
}

public interface IProfitRepository
{
    Task<ProfitSourceData> GetSourceDataAsync(DateTime fromDate, DateTime toDate);
}

public interface IEmployeeCommissionRepository
{
    Task<long> CreateAsync(EmployeeCommissionRequest request);
    Task<IEnumerable<EmployeeCommissionResponse>> GetAllAsync(string? userId, DateTime? fromDate, DateTime? toDate);
}

public interface IInvestorRepository
{
    Task<int> CreateAsync(InvestorRequest request);
    Task<IEnumerable<InvestorResponse>> GetAllAsync();
    Task<int> DistributeProfitAsync(ProfitDistributionRequest request, decimal netProfit);
    Task<IEnumerable<ProfitDistributionResponse>> GetDistributionsAsync(short? year, byte? month, int? investorId);
}

public interface IDashboardRepository
{
    Task<DashboardResponse> GetSummaryAsync(DateTime fromDate, DateTime toDate, int lowStockThreshold);
}
