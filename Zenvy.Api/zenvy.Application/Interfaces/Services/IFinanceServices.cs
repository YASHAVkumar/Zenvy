using zenvy.application.DTOs.Dashboard;
using zenvy.application.DTOs.Finance;

namespace zenvy.application.Interfaces.Services;

public interface IExpenseService
{
    Task<int> CreateTypeAsync(ExpenseTypeRequest request);
    Task<IEnumerable<ExpenseTypeResponse>> GetTypesAsync();
    Task<long> CreateExpenseAsync(ExpenseRequest request);
    Task<IEnumerable<ExpenseResponse>> GetExpensesAsync(DateTime? fromDate, DateTime? toDate, int? expenseTypeId);
}

public interface IProfitService
{
    Task<ProfitSummaryResponse> GetSummaryAsync(DateTime fromDate, DateTime toDate);
}

public interface IEmployeeCommissionService
{
    Task<long> CreateAsync(EmployeeCommissionRequest request);
    Task<IEnumerable<EmployeeCommissionResponse>> GetAllAsync(string? userId, DateTime? fromDate, DateTime? toDate);
}

public interface IInvestorService
{
    Task<int> CreateAsync(InvestorRequest request);
    Task<IEnumerable<InvestorResponse>> GetAllAsync();
    Task<int> DistributeProfitAsync(ProfitDistributionRequest request);
    Task<IEnumerable<ProfitDistributionResponse>> GetDistributionsAsync(short? year, byte? month, int? investorId);
}

public interface IDashboardService
{
    Task<DashboardResponse> GetSummaryAsync(DateTime fromDate, DateTime toDate, int lowStockThreshold);
}
