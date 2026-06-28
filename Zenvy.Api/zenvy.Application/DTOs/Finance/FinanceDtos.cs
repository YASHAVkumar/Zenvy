namespace zenvy.application.DTOs.Finance;

public class ExpenseTypeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ExpenseTypeResponse : ExpenseTypeRequest
{
    public int ExpenseTypeId { get; set; }
}

public class ExpenseRequest
{
    public int ExpenseTypeId { get; set; }
    public int POId{ get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = string.Empty;
}

public class ExpenseResponse : ExpenseRequest
{
    public long ExpenseId { get; set; }
    public string ExpenseTypeName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}

public class ProfitSummaryResponse
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal GrossSales { get; set; }
    public decimal SalesReturns { get; set; }
    public decimal NetSales { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal ProductPurchaseInvestment { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal Expenses { get; set; }
    public decimal EmployeeCommissions { get; set; }
    public decimal NetProfit { get; set; }
    public decimal GrossMarginPercent { get; set; }
    public decimal NetMarginPercent { get; set; }
    public decimal ReturnOnInvestmentPercent { get; set; }
    public int UncostedQuantity { get; set; }
}

public class ProfitSourceData
{
    public List<PurchaseCostSource> Purchases { get; set; } = [];
    public List<SaleSource> Sales { get; set; } = [];
    public List<ReturnSource> Returns { get; set; } = [];
    public List<AmountSource> Expenses { get; set; } = [];
    public List<AmountSource> Commissions { get; set; } = [];
}

public record PurchaseCostSource(int VariantId, int Quantity, decimal UnitCost);
public record SaleSource(int VariantId, int Quantity, decimal Revenue);
public record ReturnSource(int VariantId, int Quantity, decimal RefundAmount);
public record AmountSource(decimal Amount);

public class EmployeeCommissionRequest
{
    public string UserId { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public decimal CommissionPercent { get; set; }
}

public class EmployeeCommissionResponse : EmployeeCommissionRequest
{
    public long CommissionId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InvestorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal InvestmentAmount { get; set; }
    public decimal OwnershipPercent { get; set; }
    public decimal LossPercent { get; set; }=0.0m;
    public DateTime JoinDate { get; set; } = DateTime.Now;
}

public class InvestorResponse : InvestorRequest
{
    public int InvestorId { get; set; }
    public bool Status { get; set; }
}

public class ProfitDistributionRequest
{
    public byte Month { get; set; }
    public short Year { get; set; }
    public DateTime? DistributedDate { get; set; }
    public string? Notes { get; set; }
}

public class ProfitDistributionResponse
{
    public long DistributionId { get; set; }
    public int InvestorId { get; set; }
    public string InvestorName { get; set; } = string.Empty;
    public byte Month { get; set; }
    public short Year { get; set; }
    public decimal OwnershipPercent { get; set; }
    public decimal ProfitAmount { get; set; }
    public DateTime? DistributedDate { get; set; }
    public string? Notes { get; set; }
}
