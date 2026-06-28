using zenvy.Domain.Enums;

namespace zenvy.application.DTOs.SalesOrders;

public class SalesOrderRequest
{
    public int? CustomerId { get; set; }
    public int ChannelId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ExternalOrderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public OrderStatus Status { get; set; } = OrderStatus.CONFIRMED;
    public decimal ShippingFee { get; set; }
    public List<SalesOrderLineRequest> Lines { get; set; } = [];
    public int PaymentMethodId { get; set; }
    public string? ReferenceId { get; set; }
    public PaymentStatus PayStatus { get; set; }=PaymentStatus.COMPLETED;
}

public class SalesOrderLineRequest
{
    public int VariantId { get; set; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
}

public class SalesOrderResponse
{
    public long OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string? ExternalOrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SalesOrderLineResponse> Lines { get; set; } = [];
}

public class SalesOrderLineResponse
{
    public long OrderLineId { get; set; }
    public long OrderId { get; set; }
    public int ProductMasterId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int VariantId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal LineTotal { get; set; }
}
