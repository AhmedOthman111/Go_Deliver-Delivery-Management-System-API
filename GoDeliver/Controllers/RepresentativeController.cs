using GoDeliver.Application.Interfaces;
using GoDeliver.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoDeliver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Representative")]
    public class RepresentativeController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        public RepresentativeController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }
        private Guid GetAppUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId);
        }



        /// <summary>
        /// Retrieves all shipments currently assigned to the logged-in representative.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a representative to view shipments they are responsible for, 
        /// including those awaiting collection, in transit, or delivered.
        /// </remarks>
        [HttpGet("my-shipments")]
        public async Task<IActionResult> GetMyShipments()
        {
            var repId = GetAppUserId();

            var shipments = await _shipmentService.GetMyShipmentsAsync(repId);
            return Ok(shipments);
        }



        /// <summary>
        /// Marks a shipment as selected for collection by the representative.
        /// </summary>
        /// <remarks>
        /// The shipment status will be updated to <b>RepOnWayToCollect</b>, 
        /// and the representative's status will be changed to <b>OnDelivery</b>.
        /// </remarks>
        /// <param name="shipmentId">Unique identifier of the shipment to be collected.</param>
        /// <returns>Confirmation message indicating successful status update.</returns>
        [HttpPut("select-shipment/{shipmentId}")]
        public async Task<IActionResult> SelectShipmentToCollect([FromRoute] Guid shipmentId)
        {
            var repId = GetAppUserId();
            string API = "select-shipment";

            await _shipmentService.UpdateMyShipmentStatusAsync(repId, shipmentId, ShipmentStatus.RepOnWayToCollect , API);
            await _shipmentService.ChangeRepresenatativeStatueToOnDlivery(repId);
            return Ok($"Shipment {shipmentId} status updated to {ShipmentStatus.RepOnWayToCollect.ToString()}.");
        }



        /// <summary>
        /// Updates the status of a shipment to "Collected".
        /// </summary>
        /// <remarks>
        /// This endpoint is called after the representative successfully collects 
        /// the shipment from the sender and  handle mail notfecations .
        /// </remarks>
        /// <param name="shipmentId">Unique identifier of the collected shipment.</param>
        [HttpPut("shipment-collected/{shipmentId}")]
        public async Task<IActionResult> CollectShipment([FromRoute]Guid shipmentId)
        {
            string API = "shipment-collected";
            var repId = GetAppUserId();

            await _shipmentService.Collectmoney(shipmentId , API);

            await _shipmentService.UpdateMyShipmentStatusAsync(repId, shipmentId, ShipmentStatus.Collected , API);

            return Ok($"Shipment {shipmentId} status updated to {ShipmentStatus.Collected.ToString()}.");
        }



        /// <summary>
        /// Marks a shipment as being delivered to the customer.
        /// </summary>
        /// <remarks>
        /// Updates the shipment status to <b>RepOnWayToDeliver</b>, indicating that the 
        /// representative is currently on the way to the delivery address and handle mail notfecations .
        /// </remarks>
        /// <param name="shipmentId">Unique identifier of the shipment being delivered.</param>
        [HttpPut("delivering-shipment/{shipmentId}")]
        public async Task<IActionResult> OnHisWayToDeliverShipment( [FromRoute] Guid shipmentId)
        {
            string API = "delivering-shipment";
            var repId = GetAppUserId();

            await _shipmentService.UpdateMyShipmentStatusAsync(repId, shipmentId, ShipmentStatus.RepOnWayToDeliver, API);
            return Ok($"Shipment {shipmentId} status updated to {ShipmentStatus.RepOnWayToDeliver.ToString()}.");
        }



        /// <summary>
        /// Marks a shipment as successfully delivered to the customer.
        /// </summary>
        /// <remarks>
        /// Updates the shipment status to <b>Delivered</b> and changes the 
        /// representative’s availability status to <b>Available</b> for new assignments.
        /// </remarks>
        /// <param name="shipmentId">Unique identifier of the delivered shipment.</param>
        [HttpPut("shipment-delivered/{shipmentId}")]
        public async Task<IActionResult> DeliverShipmentToCustomer([FromRoute] Guid shipmentId)
        {
            var repId = GetAppUserId();

            string API = "shipment-delivered";

            await _shipmentService.Collectmoney(shipmentId, API);

            await _shipmentService.UpdateMyShipmentStatusAsync(repId, shipmentId, ShipmentStatus.Delivered, API);

            await _shipmentService.ChangeRepresenatativeStatueToAvalable(repId);

            return Ok($"Shipment {shipmentId} status updated to {ShipmentStatus.Delivered.ToString()}.");
        }

    }
}
