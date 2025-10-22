using GoDeliver.Application.Interfaces;
using GoDeliver.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoDeliver.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("api/[controller]")]
    [ApiController]
    public class RecipientController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        public RecipientController( IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }
        private Guid GetAppUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId);
        }



        /// <summary>
        /// Retrieves all shipments that are waiting for recipient  approval.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of shipments that have been created by customers (sender)
        /// and are currently awaiting approval from a recipient  That will pay.
        /// </remarks>
        [HttpGet("WaitingApproval-shipments")]
        public async Task<IActionResult> GetWaitingReciptionApprovalShipments()
        {
            var appUserId = GetAppUserId();

            var shipments = await _shipmentService.GetWaitingReciptionApprovalShipmentsAsync(appUserId);

            return Ok(shipments);
        }



        /// <summary>
        /// Accepts a shipment by recipient  and updates its status to "Under review".
        /// </summary>
        /// <remarks>
        /// This endpoint is used by a recipient to accept the  receipt
        /// of a shipment from a sender and perform payment.
        /// A payment method must be specified when accepting the shipment.
        /// </remarks>
        /// <param name="shipmentId">The unique identifier of the shipment to accept.</param>
        /// <param name="method">The selected payment method (e.g., Cash, Credit, etc.).</param>
        [HttpPut("accept-shipment/{shipmentId}")]
        public async Task<IActionResult> AcceptShipment([FromRoute]Guid shipmentId ,[FromQuery] PaymentMethod method)
        {
            var receptionistId =  GetAppUserId();
            
            await _shipmentService.AcceptShipmentAsync(shipmentId, receptionistId , method);

            return Ok("Shipment accepted successfully .");
        }



        /// <summary>
        /// Rejects a shipment by recipient .
        /// </summary>
        /// <remarks>
        /// This endpoint allows a recipient to decline a shipment and updates its status to "Canceled" .  
        /// After rejection.
        /// </remarks>
        /// <param name="shipmentId">The unique identifier of the shipment to reject.</param>
        [HttpPut("reject-shipment/{shipmentId}")]
        public async Task<IActionResult> RejectShipment(Guid shipmentId)
        {

            var receptionistId =  GetAppUserId();
            
            await _shipmentService.RejectShipmentAsync(shipmentId, receptionistId);

            return Ok("Shipment rejected successfully.");
        }

    }
}
