// EXAMPLE: Migrating SalesOrderDtos.cs to use Enums
// This shows the before/after pattern for updating DTOs

namespace zenvy.Application.DTOs.SalesOrders;

using zenvy.Domain.Enums;
using System.Text.Json.Serialization;

// ============= UPDATED REQUEST DTO =============
public class SalesOrderRequestWithEnum
{
    public int? CustomerId { get; set; }
    public int ChannelId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ExternalOrderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    // CHANGED: From 'string' to 'OrderStatus' enum
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; set; } = OrderStatus.CONFIRMED;
    
    public decimal ShippingFee { get; set; }
    public List<SalesOrderLineRequest> Lines { get; set; } = [];
}

// ============= UPDATED RESPONSE DTO =============
public class SalesOrderResponseWithEnum
{
    public long OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string? ExternalOrderId { get; set; }
    public DateTime OrderDate { get; set; }
    
    // CHANGED: From 'string' to 'OrderStatus' enum
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus Status { get; set; }
    
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SalesOrderLineResponse> Lines { get; set; } = [];
}

// ============= REPOSITORY MAPPING EXAMPLE =============
/*
When mapping from database:

    BEFORE (String-based):
    Status = reader.GetString(reader.GetOrdinal("Status")),

    AFTER (Enum-based):
    Status = (OrderStatus)Enum.Parse(
        typeof(OrderStatus), 
        reader.GetString(reader.GetOrdinal("Status")), 
        ignoreCase: true
    ),
    
    OR using EnumHelper:
    Status = EnumHelper.FromStringValue<OrderStatus>(
        reader.GetString(reader.GetOrdinal("Status"))
    ),
*/

// ============= SERVICE USAGE EXAMPLE =============
/*
public class SalesOrderService
{
    public async Task<SalesOrderResponseWithEnum> CreateOrderAsync(SalesOrderRequestWithEnum request)
    {
        // Input validation using enum
        var validStatuses = new[] { OrderStatus.PENDING, OrderStatus.CONFIRMED };
        if (!validStatuses.Contains(request.Status))
            throw new ArgumentException("Invalid order status");
        
        var order = new Order 
        { 
            Status = request.Status, // Type-safe assignment
            ChannelId = request.ChannelId,
            OrderDate = request.OrderDate
        };
        
        await _repository.AddAsync(order);
        
        // Response includes enum, automatically serialized to JSON string
        return MapToDto(order);
    }
    
    public async Task<SalesOrderResponseWithEnum> UpdateOrderStatusAsync(
        long orderId, 
        OrderStatus newStatus) // Accepts only valid enum values
    {
        var order = await _repository.GetByIdAsync(orderId);
        order.Status = newStatus;
        await _repository.UpdateAsync(order);
        return MapToDto(order);
    }
}
*/

// ============= KEY CHANGES =============
/*
1. Replace 'string Status' with '[JsonConverter]public OrderStatus Status'
2. Set default: = OrderStatus.CONFIRMED (instead of = "CONFIRMED")
3. Add 'using zenvy.Domain.Enums;'
4. Add 'using System.Text.Json.Serialization;'
5. Add [JsonConverter(typeof(JsonStringEnumConverter))] attribute
   - Automatically converts enum to/from JSON string during serialization
6. Update repository mappings to use EnumHelper.FromStringValue<T>()
7. Update services to use enum values directly

JSON Serialization Examples:
- C# Enum: OrderStatus.DELIVERED
- JSON: { "status": "DELIVERED" }
- Query Parameter: /api/orders?status=DELIVERED

The [JsonConverter] attribute handles both directions:
- Enum → JSON string (when serializing response)
- JSON string → Enum (when deserializing request)
*/
