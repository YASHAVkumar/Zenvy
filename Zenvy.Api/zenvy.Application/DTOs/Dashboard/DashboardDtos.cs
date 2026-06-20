namespace zenvy.application.DTOs.Dashboard;

public class DashboardResponse
{
    public DashboardKpis Kpis { get; set; } = new();
    public List<SalesTrendPoint> SalesTrend { get; set; } = [];
    public List<TopProductItem> TopProducts { get; set; } = [];
    public List<ExpenseBreakdownItem> ExpenseBreakdown { get; set; } = [];
    public List<LowStockItem> LowStockItems { get; set; } = [];
}

public class DashboardKpis
{
    public decimal Revenue { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProductPurchaseInvestment { get; set; }
    public decimal NetMarginPercent { get; set; }
    public decimal ReturnOnInvestmentPercent { get; set; }
    public int UncostedQuantity { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalExpenses { get; set; }
    public int LowStockCount { get; set; }
}

public class SalesTrendPoint
{
    public DateTime Period { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class TopProductItem
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class ExpenseBreakdownItem
{
    public int ExpenseTypeId { get; set; }
    public string ExpenseTypeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class LowStockItem
{
    public int VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int AvailableQty { get; set; }
}
