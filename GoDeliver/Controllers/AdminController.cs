using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.PricingRuleService;
using GoDeliver.Dtos;
using GoDeliver.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoDeliver.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ICustomerService _customerService;
        private readonly IPricingRuleService _pricingRuleService;
        public AdminController(IShipmentService shipmentService , ICustomerService customerService, IPricingRuleService pricingRuleService)
        {
            _shipmentService = shipmentService;
            _customerService = customerService;
            _pricingRuleService = pricingRuleService;
        }

        private Guid GetAppUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId);
        }



        /// <summary>
        /// Retrieves all shipments in the system.
        /// </summary>
        /// <remarks>
        /// Returns a list of all shipments regardless of their status.  
        /// Accessible only by administrators.
        /// </remarks>
        [HttpGet("shipments")]
        public async Task<IActionResult> GetAllShipments()
        {
            var shipments = await _shipmentService.GetAllShipmentsAsync();

            return Ok(shipments);
        }



        /// <summary>
        /// Retrieves all canceled shipments.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to view shipments that were canceled by either customers or system users.
        /// </remarks>
        [HttpGet("shipments/canceled")]
        public async Task<IActionResult> GetCanceledShipments()
        {
            var shipments = await _shipmentService.GetCanceledShipments();
            return Ok(shipments);
        }


        /// <summary>
        /// Searches for a shipment using its tracking number.
        /// </summary>
        /// <remarks>
        /// Returns detailed shipment information if the tracking number exists.
        /// </remarks>
        /// <param name="trackingNumber">The tracking number of the shipment to search for.</param>
        [HttpGet("shipments/search/{trackingNumber}")]
        public async Task<IActionResult> SearchShipmentByTracking( [FromRoute] string trackingNumber)
        {
            var shipment = await _shipmentService.SearchAboutShipmentByTrackNumber(trackingNumber);
            return Ok(shipment);
        }




        /// <summary>
        /// Updates the status of a shipment.
        /// </summary>
        /// <remarks>
        /// Admins can change shipment statuses manually in exceptional cases
        /// (e.g., system errors or manual overrides).
        /// </remarks>
        /// <param name="dto">Object containing the shipment ID and the new status to apply.</param>
        [HttpPut("shipments/update-status")]
        public async Task<IActionResult> UpdateShipmentStatus([FromBody] UpdateShipmentStatusDto dto)
        {
            var adminId =  GetAppUserId();
           await _shipmentService.UpdateShipmentStatusAsync(adminId, dto.ShipmentId, dto.NewStatus,"changed by admin");

            return Ok($"Shipment status updated to {dto.NewStatus}.");
        }



        /// <summary>
        /// Retrieves all registered customers in the system.
        /// </summary>
        /// <remarks>
        /// This includes customer account information and linked user data.
        /// </remarks>
        [HttpGet("cusotmers")]
        public async Task<IActionResult> GeAllCustomer()
        {
            var customers = await _customerService.GetAllCustomers();
            return Ok(customers);
        }




        /// <summary>
        /// Retrieves all pricing rules.
        /// </summary>
        /// <remarks>
        /// Pricing rules determine shipment cost calculations based on regions, weight, or dimensions.
        /// </remarks>
        [HttpGet("pricing-rules")]
        public async Task<IActionResult> GetAllPricingRules()
        {
            var rules = await _pricingRuleService.GetAllPricingRulesAsync();
            return Ok(rules);
        }



        /// <summary>
        /// Creates a new pricing rule.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to define how shipment pricing should be calculated for specific conditions.
        /// </remarks>
        /// <param name="dto">Data for the new pricing rule, including weight and base price.</param>
        [HttpPost("pricing-rules")]
        public async Task<IActionResult> CreatePricingRule([FromBody] CreatePricingDto dto)
        {
            var createdRule = await _pricingRuleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAllPricingRules), new { id = createdRule.Id }, createdRule);
        }



        /// <summary>
        /// Updates an existing pricing rule.
        /// </summary>
        /// <remarks>
        /// Modify pricing rule values like rate per kilometer, base cost, or other configuration fields.
        /// </remarks>
        /// <param name="id">The ID of the pricing rule to update.</param>
        /// <param name="dto">The updated pricing rule data.</param>
        [HttpPut("pricing-rules/{id}")]
        public async Task<IActionResult> UpdatePricingRule(Guid id, [FromBody] CreatePricingDto dto)
        {

            var updated = await _pricingRuleService.UpdateAsync(id, dto);
            return Ok(updated);
        }



        /// <summary>
        /// Deletes an existing pricing rule.
        /// </summary>
        /// <remarks>
        /// This operation removes a pricing rule permanently from the system.  
        /// Use with caution as it may affect shipment cost calculations.
        /// </remarks>
        /// <param name="id">The unique identifier of the pricing rule to delete.</param>
        /// <returns>No content response on successful deletion.</returns>
        [HttpDelete("pricing-rules/{id}")]
        public async Task<IActionResult> DeletePricingRule(Guid id)
        {
            await _pricingRuleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
