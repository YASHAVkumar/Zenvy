using zenvy.Domain.Enums;

namespace zenvy.application.DTOs.PurchaseOrders;

public class PurchaseOrderRequest
{
    public int SupplierId { get; set; }
    public int WarehouseId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public DateTime? ExpectedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.RECEIVED;
    public string CreatedBy { get; set; } = string.Empty;
    public List<PurchaseOrderLineRequest> Lines { get; set; } = [];
    public List<ExpenseRequestExt> Expenses { get; set; } = [];
}

public class ExpenseRequestExt
{
    public int ExpenseTypeId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
}
public class PurchaseOrderLineRequest
{
    public int VariantId { get; set; }
    public int Qty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxAmount { get; set; }
}

public class PurchaseOrderResponse
{
    public long POId { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string PONumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PurchaseOrderLineResponse> Lines { get; set; } = [];
}

public class PurchaseOrderLineResponse
{
    public long POLineId { get; set; }
    public long POId { get; set; }
    public int ProductMasterId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}
