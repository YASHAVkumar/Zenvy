using zenvy.application.DTOs.Shipments;
using zenvy.application.Interfaces.Repositories;
using zenvy.application.Interfaces.Services;

namespace zenvy.Application.Services;

public class ShipmentService(IShipmentRepository shipmentRepository) : IShipmentService
{
    public Task<long> CreateShipmentAsync(ShipmentRequest request)
    {
        return shipmentRepository.CreateAsync(request);
    }

    public Task<IEnumerable<ShipmentResponse>> GetShipmentsAsync()
    {
        return shipmentRepository.GetAllAsync();
    }
}
