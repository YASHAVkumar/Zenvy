using zenvy.application.DTOs.Shipments;

namespace zenvy.application.Interfaces.Services;

public interface IShipmentService
{
    Task<long> CreateShipmentAsync(ShipmentRequest request);
    Task<IEnumerable<ShipmentResponse>> GetShipmentsAsync();
}
