namespace zenvy.Domain.Enums;

/// <summary>
/// Enum to String Conversion Map
/// This provides mappings between enum values and their database/API string representations
/// </summary>
public static class EnumMappings
{
    /// <summary>
    /// Maps OrderStatus enums to their database string values
    /// </summary>
    public static readonly Dictionary<OrderStatus, string> OrderStatusMap = new()
    {
        { OrderStatus.PENDING, "PENDING" },
        { OrderStatus.CONFIRMED, "CONFIRMED" },
        { OrderStatus.PROCESSING, "PROCESSING" },
        { OrderStatus.PACKED, "PACKED" },
        { OrderStatus.SHIPPED, "SHIPPED" },
        { OrderStatus.DELIVERED, "DELIVERED" },
        { OrderStatus.CANCELLED, "CANCELLED" }
    };

    /// <summary>
    /// Maps ReturnStatus enums to their database string values
    /// </summary>
    public static readonly Dictionary<ReturnStatus, string> ReturnStatusMap = new()
    {
        { ReturnStatus.REQUESTED, "REQUESTED" },
        { ReturnStatus.APPROVED, "APPROVED" },
        { ReturnStatus.REJECTED, "REJECTED" },
        { ReturnStatus.PROCESSING, "PROCESSING" },
        { ReturnStatus.COMPLETED, "COMPLETED" }
    };

    /// <summary>
    /// Maps RefundStatus enums to their database string values
    /// </summary>
    public static readonly Dictionary<RefundStatus, string> RefundStatusMap = new()
    {
        { RefundStatus.PENDING, "PENDING" },
        { RefundStatus.INITIATED, "INITIATED" },
        { RefundStatus.PROCESSED, "PROCESSED" },
        { RefundStatus.FAILED, "FAILED" },
        { RefundStatus.CANCELLED, "CANCELLED" }
    };

    /// <summary>
    /// Maps PurchaseOrderStatus enums to their database string values
    /// </summary>
    public static readonly Dictionary<PurchaseOrderStatus, string> PurchaseOrderStatusMap = new()
    {
        { PurchaseOrderStatus.DRAFT, "DRAFT" },
        { PurchaseOrderStatus.SUBMITTED, "SUBMITTED" },
        { PurchaseOrderStatus.CONFIRMED, "CONFIRMED" },
        { PurchaseOrderStatus.RECEIVED, "RECEIVED" },
        { PurchaseOrderStatus.INVOICED, "INVOICED" },
        { PurchaseOrderStatus.PAID, "PAID" },
        { PurchaseOrderStatus.CANCELLED, "CANCELLED" },
        { PurchaseOrderStatus.PENDING, "PENDING" }
    };

    /// <summary>
    /// Maps MarketplaceSettlementStatus enums to their database string values
    /// </summary>
    public static readonly Dictionary<MarketplaceSettlementStatus, string> SettlementStatusMap = new()
    {
        { MarketplaceSettlementStatus.PENDING, "PENDING" },
        { MarketplaceSettlementStatus.PROCESSED, "PROCESSED" },
        { MarketplaceSettlementStatus.SETTLED, "SETTLED" },
        { MarketplaceSettlementStatus.FAILED, "FAILED" },
        { MarketplaceSettlementStatus.DISPUTED, "DISPUTED" },
        { MarketplaceSettlementStatus.REVERSED, "REVERSED" }
    };

    /// <summary>
    /// Maps ChannelType enums to their display names
    /// </summary>
    public static readonly Dictionary<ChannelType, string> ChannelTypeMap = new()
    {
        { ChannelType.MEESHO, "Meesho" },
        { ChannelType.MYNTRA, "Myntra" },
        { ChannelType.AMAZON, "Amazon" },
        { ChannelType.WEBSITE, "Website" },
        { ChannelType.WALK_IN, "Walk-In" },
        { ChannelType.OTHER, "Other" }
    };
    // {PaymentStatus.PENDING,"PENDING"},
    // {PaymentStatus.INITIATED,"INITIATED},
    // {PaymentStatus.AUTHORIZED,"AUTHORIZED"},
    // {PaymentStatus.CAPTURED,"CAPTURED"},
    // {PaymentStatus.COMPLETED,"COMPLETED"},
    // {PaymentStatus.FAILED,"FAILED"},
    // {PaymentStatus.CANCELLED,"CANCELLED" },
    // {PaymentStatus.REFUNDED,"REFUNDED" }
    public static readonly Dictionary<PaymentStatus, string> PaymentStatusMap = new()
    {
        {PaymentStatus.PENDING,"PENDING"},
        {PaymentStatus.INITIATED,"INITIATED"},
        {PaymentStatus.AUTHORIZED,"AUTHORIZED"},
        {PaymentStatus.CAPTURED,"CAPTURED"},
        {PaymentStatus.COMPLETED,"COMPLETED"},
        {PaymentStatus.FAILED,"FAILED"},
        {PaymentStatus.CANCELLED,"CANCELLED" },
        {PaymentStatus.REFUNDED,"REFUNDED" }
    };


    /// <summary>
    /// Get string value from enum
    /// </summary>
    public static string GetOrderStatusValue(OrderStatus status) => OrderStatusMap.GetValueOrDefault(status, status.ToString());

    /// <summary>
    /// Get string value from enum
    /// </summary>
    public static string GetReturnStatusValue(ReturnStatus status) => ReturnStatusMap.GetValueOrDefault(status, status.ToString());

    /// <summary>
    /// Get string value from enum
    /// </summary>
    public static string GetRefundStatusValue(RefundStatus status) => RefundStatusMap.GetValueOrDefault(status, status.ToString());

    /// <summary>
    /// Get string value from enum
    /// </summary>
    public static string GetPurchaseOrderStatusValue(PurchaseOrderStatus status) => PurchaseOrderStatusMap.GetValueOrDefault(status, status.ToString());

    /// <summary>
    /// Get string value from enum
    /// </summary>
    public static string GetSettlementStatusValue(MarketplaceSettlementStatus status) => SettlementStatusMap.GetValueOrDefault(status, status.ToString());

    /// <summary>
    /// Get display name from channel type
    /// </summary>
    public static string GetChannelTypeName(ChannelType channelType) => ChannelTypeMap.GetValueOrDefault(channelType, channelType.ToString());
   public static string GetPaymentStatusValue(PaymentStatus status) => PaymentStatusMap.GetValueOrDefault(status, status.ToString());
}
