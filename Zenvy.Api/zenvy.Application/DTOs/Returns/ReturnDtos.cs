using zenvy.Domain.Enums;

namespace zenvy.application.DTOs.Returns;

public class ReturnRequest
{
    public long OrderId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.Now;
    public ReturnReason? Reason { get; set; }= ReturnReason.OTHER;
    public RefundStatus RefundStatus { get; set; } = Domain.Enums.RefundStatus.PROCESSED;
    public ReturnStatus Status { get; set; } = ReturnStatus.COMPLETED;
    public string? RefundMethod { get; set; }
   // public decimal RefundAmount { get; set; }
    public decimal ReturnShippingFee { get; set; }
    public decimal MarketplaceFee { get; set; }
    public bool DeliveryFeeRefunded { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }
    public List<ReturnLineRequest> Lines { get; set; } = [];
}

public class ReturnLineRequest
{
    public long OrderLineId { get; set; }
    public int Qty { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Condition { get; set; }
    public bool Restock { get; set; } = true;
    public int? RestockWarehouseId { get; set; }
}

public class ReturnStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? RefundStatus { get; set; }
    public string? Notes { get; set; }
}

public class ReturnResponse
{
    public long ReturnId { get; set; }
    public long OrderId { get; set; }
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string? ExternalOrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RefundStatus { get; set; } = string.Empty;
    public string? RefundMethod { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal ReturnShippingFee { get; set; }
    public decimal MarketplaceFee { get; set; }
    public bool DeliveryFeeRefunded { get; set; }
    public string? CreatedBy { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReturnLineResponse> Lines { get; set; } = [];
}

public class ReturnLineResponse
{
    public long ReturnLineId { get; set; }
    public long ReturnId { get; set; }
    public long OrderLineId { get; set; }
    public int ProductMasterId { get; set; }
    public int VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal RefundAmount { get; set; }
    public string? Condition { get; set; }
    public bool Restock { get; set; }
    public int? RestockWarehouseId { get; set; }
    public string? RestockWarehouseName { get; set; }
}
