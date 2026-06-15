namespace zenvy.application.DTOs.Shipments;

public class ShipmentRequest
{
    public long OrderId { get; set; }
    public string? CourierName { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? Status { get; set; }
}

public class ShipmentResponse
{
    public long ShipmentId { get; set; }
    public long OrderId { get; set; }
    public string? CourierName { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ShippedDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public string? Status { get; set; }
}
