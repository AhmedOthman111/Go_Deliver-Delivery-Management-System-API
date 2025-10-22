using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.Customerservice;
using GoDeliver.Application.Models.shared;
using GoDeliver.Application.Models.ShipmentService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoDeliver.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService  _customerService;
        private readonly IShipmentService _shipmentService;
        private readonly IPaymentService _paymentService;
        public CustomerController(ICustomerService customerService , IShipmentService shipmentService , IPaymentService paymentService )
        {
            _customerService = customerService;
            _shipmentService = shipmentService;
            _paymentService = paymentService;
        }
        private Guid GetAppUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userId);
        }




        /// <summary>
        /// Retrieves the current logged-in customer's profile details.
        /// </summary>
        /// <remarks>
        /// This endpoint returns personal information such as name, email, phone number, and gender 
        /// for the authenticated customer. The user must be logged in to access this information.
        /// </remarks>
        /// <returns>Customer profile information.</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var appUserId = GetAppUserId();
            var profile = await _customerService.GetProfileAsync(appUserId);
            return Ok(profile);
        }




        /// <summary>
        /// Updates the customer's profile information.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to update personal data such as name, gender, phone number, etc.
        /// The user must be authenticated and have the Customer role.
        /// </remarks>
        /// <param name="dto">The updated customer information.</param>
        [HttpPut("Update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpateCustomerInfoDto dto)
        {
            var appUserId = GetAppUserId();
            await _customerService.UpdateCustomerinfoAsync(appUserId, dto);
            return NoContent(); 
        }



        /// <summary>
        /// Retrieves all saved addresses for the current customer.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to Get a list of all addresses associated with the logged-in customer.
        /// </remarks>
        [HttpGet("addresses")]
        public async Task<IActionResult> GetAddresses()
        {
            var appUserId = GetAppUserId();
            var addresses = await _customerService.GetAddressesAsync(appUserId);
            return Ok(addresses);
        }



        /// <summary>
        /// Adds a new address for the current customer.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to add a new delivery or pickup address to your profile.
        /// </remarks>
        /// <param name="dto">Address details such as street, city, and governorate.</param>
        [HttpPost("add-address")]
        public async Task<IActionResult> AddAddress([FromBody] AddressDto dto)
        {
            var appUserId = GetAppUserId();
            await _customerService.AddAddressAsync(appUserId, dto);
            return Ok(new { message = "Address added successfully" });
        }



        /// <summary>
        /// Updates an existing address for the current customer.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to modify an address already stored in your account.
        /// </remarks>
        /// <param name="addressId">The unique identifier of the address to update.</param>
        /// <param name="dto">The new address details.</param>
        [HttpPut("addresses/{addressId:guid}")]
        public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] AddressDto dto)
        {
            var appUserId = GetAppUserId();
            await _customerService.UpdateAddressAsync(appUserId, addressId, dto);
            return Ok(new { message = "Address updated successfully" });
        }



        /// <summary>
        /// Deletes an existing address from the customer's account.
        /// </summary>
        /// <remarks>
        /// This action permanently removes the specified address.
        /// </remarks>
        /// <param name="addressId">The unique identifier of the address to delete.</param>
        [HttpDelete("addresses/{addressId:guid}")]
        public async Task<IActionResult> DeleteAddress(Guid addressId)
        {
            var appUserId = GetAppUserId();
            await _customerService.DeleteAddressAsync(appUserId, addressId);
            return Ok(new { message = "Address deleted successfully" });
        }



        /// <summary>
        /// Creates a new shipment for the logged-in customer.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to create a shipment request by specifying (sender), receiver, package details, 
        /// and payment information. A tracking number will be generated automatically.
        /// </remarks>
        /// <param name="dto">Shipment creation data including addresses and payment type.</param>
        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment( [FromBody] CreateShipmentDto dtp)
        {
            var appUserId = GetAppUserId();
            
            var tracknumber =  await _shipmentService.CreateShipmentAsync(appUserId, dtp);

            return Ok(new { message = $"Shimpment Created successfully . Tracking number: {tracknumber} " });
        }



        /// <summary>
        /// Retrieves all shipments related to current customer as sender or receptiont.
        /// </summary>
        /// <remarks>
        /// Returns all active, canceled, or completed shipments belonging to the logged-in customer.
        /// </remarks>
        [HttpGet("my-shipments")]
        public async Task<IActionResult> GetMyShipments()
        {
            var userId = GetAppUserId();
            var shipments = await _shipmentService.GetAllShipmentsByCustomerAsync(userId);
            return Ok(shipments);
        }



        /// <summary>
        /// Tracks a shipment using its tracking number.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to check the status and details of a specific shipment.
        /// </remarks>
        /// <param name="trackingNumber">Unique tracking number of the shipment.</param>
        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> TrackShipment([FromRoute] string trackingNumber)
        {
            var userId = GetAppUserId();

            var shipment = await _shipmentService.GetShipmentByTrackingAsync(trackingNumber, userId);

            return Ok(shipment);
        }




        /// <summary>
        /// Cancels a shipment before it is picked up or processed.
        /// </summary>
        /// <remarks>
        /// A customer can cancel a shipment only if it hasn't been accepted or dispatched yet.
        /// </remarks>
        /// <param name="shipmentId">The unique identifier of the shipment to cancel.</param>
        [HttpPut("cancel-shipment/{shipmentId}")]
        public async Task<IActionResult> CancelShipment(Guid shipmentId)
        {
            var userId = GetAppUserId();
           
            await _shipmentService.CancelShipmentAsync(shipmentId, userId);

            return Ok("Shipment canceled successfully.");
        }



        /// <summary>
        /// Retrieves all payment transactions for the current customer.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to view your payment history, including completed and pending payments for shipments.
        /// </remarks>
        [HttpGet("Payments")]
        public async Task<IActionResult> CustomerTransaction()
        {
            var userId = GetAppUserId();

            var transactions =  await _paymentService.GetCustomerPaymentsAsync(userId);

            return Ok(transactions);
        }

    }
}
