using GoDeliver.Application.Models.ShipmentService;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface IShipmentService
    {
        Task<string> CreateShipmentAsync(Guid appUserId, CreateShipmentDto dto);
        Task AcceptShipmentAsync(Guid shipmentId, Guid recipientAppUserId, PaymentMethod paymentMethod);

        Task<IEnumerable<ShipmentDto>> GetAllShipmentsByCustomerAsync(Guid appUserId);
        Task<ShipmentDetailsDto> GetShipmentByTrackingAsync(string trackingNumber, Guid appUserId);
        Task CancelShipmentAsync(Guid shipmentId, Guid appUserId);
        Task<IEnumerable<ShipmentDto>> GetAllShipmentsAsync();
        Task AssignRepresentativeAsync(Guid shipmentId, Guid representativeAppUserId);
        Task UpdateShipmentStatusAsync(Guid userid, Guid shipmentId, ShipmentStatus status, string notes);
         Task<IEnumerable<ShipmentDto>> GetUnderReviewingShipmentsAsync(Guid EmployeeID);
        Task<IEnumerable<ShipmentDto>> GetWaitingReciptionApprovalShipmentsAsync(Guid ReciptionappUserId);
        Task<ShipmentDetailsDto> SearchAboutShipmentByTrackNumber(string trackingNumber);
        Task<IEnumerable<ShipmentDto>> GetCanceledShipments();
        Task<IEnumerable<representiveShipmentsDto>> GetMyShipmentsAsync(Guid repAppUserId);
        Task UpdateMyShipmentStatusAsync(Guid repAppUserId, Guid shipmentId, ShipmentStatus newStatus, string API, string? notes = null);
        Task<IEnumerable<RepresentativeDto>> GetAvelableRepresentativeInEmployeeGovernate(Guid employeeID);
        Task ChangeRepresenatativeStatueToAvalable(Guid RepId);
        Task RejectShipmentAsync(Guid shipmentId, Guid recipientAppUserId);
        Task ChangeRepresenatativeStatueToOnDlivery(Guid RepId);
        Task Collectmoney(Guid shipmentid, string API);
    }
}
