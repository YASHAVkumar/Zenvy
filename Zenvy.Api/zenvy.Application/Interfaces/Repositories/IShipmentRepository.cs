using zenvy.application.DTOs.Shipments;

namespace zenvy.application.Interfaces.Repositories;

public interface IShipmentRepository
{
    Task<long> CreateAsync(ShipmentRequest request);
    Task<IEnumerable<ShipmentResponse>> GetAllAsync();
}
