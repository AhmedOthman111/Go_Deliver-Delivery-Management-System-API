using GoDeliver.Application.Interfaces;
using GoDeliver.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoDeliver.Controllers
{

    [Authorize(Roles = "Employee")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public EmployeeController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        private Guid GetAppUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId);
        }



        /// <summary>
        /// assign representative to shipment 
        /// </summary>
        /// <remarks>
        /// this endpoint used by employee to assign avelable representative to the shipment that is currently under review .
        /// and  update Shipment statue to rep assigned. 
        /// </remarks>
        /// <param name="dto"> Contains shipment ID and representative ID to assign </param>
        [HttpPut("assign-representative")]
        public async Task<IActionResult> AssignRepresentative([FromBody] AssignRepresentativeDto dto)
        {

            var employeeId =  GetAppUserId();
            
            await _shipmentService.AssignRepresentativeAsync(dto.ShipmentId, dto.RepresentativeId);

            return Ok("Representative assigned successfully.");
        }



        /// <summary>
        /// Retrieves all available representatives in the same governorate as the employee.
        /// </summary>
        /// <remarks>
        /// Useful for employees to view which representatives are currently available 
        /// to assign them to shipments within their area.
        /// </remarks>
        [HttpGet("available-representatives")]
        public async Task<IActionResult> GetAvailableRepresentatives()
        {
            var employeeId = GetAppUserId();
           
            var representatives = await _shipmentService.GetAvelableRepresentativeInEmployeeGovernate(employeeId);
           
            return Ok(representatives);
        }



        /// <summary>
        /// Retrieves all shipments currently under review for the logged-in employee.
        /// </summary>
        /// <remarks>
        /// This endpoint helps employees monitor and manage shipments 
        /// that are still in the "under review" state and need to assign to representative.
        /// </remarks>
        [HttpGet("under-review-shipments")]
        public async Task<IActionResult> GetUnderReviewShipments()
        {
            var employeeId = GetAppUserId();

            var shipments = await _shipmentService.GetUnderReviewingShipmentsAsync(employeeId);

            return Ok(shipments);
        }



        /// <summary>
        /// Searches for a shipment using its tracking number.
        /// </summary>
        /// <remarks>
        /// Employees can use this endpoint to quickly locate shipment details 
        /// based on a unique tracking number.
        /// </remarks>
        /// <param name="trackingNumber">Unique tracking number of the shipment.</param>
        [HttpGet("search/{trackingNumber}")]
        public async Task<IActionResult> SearchShipmentByTracking(string trackingNumber)
        {
            var shipment = await _shipmentService.SearchAboutShipmentByTrackNumber(trackingNumber);
            return Ok(shipment);
        }

    }
}
