using GoDeliver.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public interface IShipmentRepository : IGenericRepository<Shipment>
    {
        Task<IEnumerable<Shipment>> GetAllByCustomerIdAsync(Guid appUserId);
        Task<Shipment> GetByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<Shipment>> GetCanceledShipments();
        Task<IEnumerable<Shipment>> GetShipmentsThatAssignedTORepresemativeAsync(Guid repAppUserId);
        Task<IEnumerable<Shipment>> GetUnderReviewingShipmentsThatSenderAddresEQEmplyeeAddresAsync(Guid EmployeeID);


    }
}
