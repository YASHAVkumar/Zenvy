namespace zenvy.application.DTOs.Inventory;

public class InventoryRequest
{
    public int VariantId { get; set; }
    public int WarehouseId { get; set; }
    public int OnHandQty { get; set; }
    public int ReservedQty { get; set; }
}

public class InventoryResponse
{
    public int InventoryId { get; set; }
    public int ProductMasterId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public int OnHandQty { get; set; }
    public int ReservedQty { get; set; }
    public int AvailableQty { get; set; }
}

public class InventoryAdjustmentRequest
{
    public int VariantId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public int? CreatedBy { get; set; }
}

public class InventoryDamageRequest
{
    public int VariantId { get; set; }
    public int WarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public int? CreatedBy { get; set; }
}

public class InventoryTransferRequest
{
    public int VariantId { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public int? CreatedBy { get; set; }
}

public class InventoryTransactionResponse
{
    public long TransactionId { get; set; }
    public int ProductMasterId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string Warehouse { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
