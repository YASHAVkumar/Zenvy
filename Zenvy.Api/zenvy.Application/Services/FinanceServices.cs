using zenvy.application.DTOs.Dashboard;
using zenvy.application.DTOs.Finance;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.application.Services;

public class ExpenseService(IExpenseRepository repository) : IExpenseService
{
    public Task<int> CreateTypeAsync(ExpenseTypeRequest request) => repository.CreateTypeAsync(request);
    public Task<IEnumerable<ExpenseTypeResponse>> GetTypesAsync() => repository.GetTypesAsync();
    public Task<long> CreateExpenseAsync(ExpenseRequest request) => repository.CreateExpenseAsync(request);
    public Task<IEnumerable<ExpenseResponse>> GetExpensesAsync(DateTime? fromDate, DateTime? toDate, int? expenseTypeId) => repository.GetExpensesAsync(fromDate, toDate, expenseTypeId);
}

public class ProfitService(IProfitRepository repository) : IProfitService
{
    public async Task<ProfitSummaryResponse> GetSummaryAsync(DateTime fromDate, DateTime toDate)
    {
        var data = await repository.GetSourceDataAsync(fromDate, toDate);
        var purchaseCosts = data.Purchases
            .GroupBy(x => x.VariantId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(x => x.Quantity) == 0
                    ? 0m
                    : group.Sum(x => x.Quantity * x.UnitCost) / group.Sum(x => x.Quantity));

        var returnedQuantity = data.Returns
            .GroupBy(x => x.VariantId)
            .ToDictionary(group => group.Key, group => group.Sum(x => x.Quantity));

        decimal investment = 0m;
        var uncostedQuantity = 0;

        foreach (var sale in data.Sales.GroupBy(x => x.VariantId))
        {
            var sold = sale.Sum(x => x.Quantity);
            var returned = returnedQuantity.GetValueOrDefault(sale.Key);
            var netQuantity = Math.Max(0, sold - returned);
            if (purchaseCosts.TryGetValue(sale.Key, out var averageCost) && averageCost > 0m)
                investment += netQuantity * averageCost;
            else
                uncostedQuantity += netQuantity;
        }

        var grossSales = data.Sales.Sum(x => x.Revenue);
        var salesReturns = data.Returns.Sum(x => x.RefundAmount);
        var netSales = grossSales - salesReturns;
        var expenses = data.Expenses.Sum(x => x.Amount);
        var commissions = data.Commissions.Sum(x => x.Amount);
        var costOfGoodsSold = investment;
        var grossProfit = netSales - costOfGoodsSold;
        var netProfit = grossProfit - expenses - commissions;

        return new ProfitSummaryResponse
        {
            FromDate = fromDate,
            ToDate = toDate,
            GrossSales = grossSales,
            SalesReturns = salesReturns,
            NetSales = netSales,
            CostOfGoodsSold = costOfGoodsSold,
            ProductPurchaseInvestment = costOfGoodsSold,
            GrossProfit = grossProfit,
            Expenses = expenses,
            EmployeeCommissions = commissions,
            NetProfit = netProfit,
            GrossMarginPercent = Percent(grossProfit, Math.Max(netSales, 0m)),
            NetMarginPercent = Percent(netProfit, Math.Max(netSales, 0m)),
            ReturnOnInvestmentPercent = Percent(netProfit, Math.Max(costOfGoodsSold, 0m)),
            UncostedQuantity = uncostedQuantity
        };
    }

    private static decimal Percent(decimal value, decimal basis) =>
        basis == 0 ? 0 : decimal.Round(value * 100 / basis, 2, MidpointRounding.AwayFromZero);
}

public class EmployeeCommissionService(IEmployeeCommissionRepository repository) : IEmployeeCommissionService
{
    public Task<long> CreateAsync(EmployeeCommissionRequest request) => repository.CreateAsync(request);
    public Task<IEnumerable<EmployeeCommissionResponse>> GetAllAsync(string? userId, DateTime? fromDate, DateTime? toDate) => repository.GetAllAsync(userId, fromDate, toDate);
}

public class EmployeeCompensationService(IEmployeeCompensationRepository repository) : IEmployeeCompensationService
{
    public Task<IEnumerable<EmployeeCompensationResponse>> GetReportAsync(DateTime fromDate, DateTime toDate, int? employeeId) =>
        repository.GetReportAsync(fromDate, toDate, employeeId);
}

public class InvestorService(IInvestorRepository repository, IProfitService profitService) : IInvestorService
{
    public Task<int> CreateAsync(InvestorRequest request) => repository.CreateAsync(request);
    public Task<IEnumerable<InvestorResponse>> GetAllAsync() => repository.GetAllAsync();
    public async Task<int> DistributeProfitAsync(ProfitDistributionRequest request)
    {
        var fromDate = new DateTime(request.Year, request.Month, 1);
        var toDate = fromDate.AddMonths(1).AddTicks(-1);
        var profit = await profitService.GetSummaryAsync(fromDate, toDate);
        if (profit.UncostedQuantity > 0)
            throw new InvalidOperationException("Profit cannot be distributed while sold products have no purchase cost history.");
        if (profit.NetProfit <= 0)
            throw new InvalidOperationException("There is no positive net profit to distribute for this period.");
        return await repository.DistributeProfitAsync(request, profit.NetProfit);
    }
    public Task<IEnumerable<ProfitDistributionResponse>> GetDistributionsAsync(short? year, byte? month, int? investorId) => repository.GetDistributionsAsync(year, month, investorId);
}
