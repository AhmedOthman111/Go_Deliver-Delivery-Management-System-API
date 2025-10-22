using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GoDeliver.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }



        /// <summary>
        /// Registers a new customer account.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to register a new customer with full required  details .
        /// The user will receive a confirmation email for account verification.
        /// </remarks>
        /// <param name="dto">Customer registration data including personal details and credentials.</param>
        [HttpPost("register-customer")]
        [AllowAnonymous]
        public async Task<ActionResult> RegisterCustomer([FromBody] CustomerRegisterDto dto )
        {
           var result =  await _authService.RegisterCustomerAsync(dto);

            return Ok(result.Message);
        }



        /// <summary>
        /// Registers a new employee account. Accessible only by admins.
        /// </summary>
        /// <remarks>
        /// This endpoint allows the admin to create employee accounts that can manage operations like approving shipments.
        /// </remarks>
        /// <param name="dto">Employee registration details including name, email, and credentials.</param>
        [HttpPost("register-employee")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> RegisterEmployee([FromBody] EmployeeRegisterDto dto)
        {
           await _authService.RegisterEmployeeAsync(dto);
            return Created();
        }



        /// <summary>
        /// Registers a new representative account. Accessible by employees or admins.
        /// </summary>
        /// <remarks>
        /// Representatives are responsible for shipment pickup and delivery. 
        /// This endpoint allows authorized employees or admins to register them.
        /// </remarks>
        /// <param name="dto">Representative registration data.</param>
        [HttpPost("register-representative")]
        [Authorize(Roles = "Employee,Admin")] 
        public async Task<IActionResult> RegisterRepresentative([FromBody] RepresentativeRegisterDto dto)
        {
           await _authService.RegisterRepresentativeAsync(dto);
            return Created();
        }



        /// <summary>
        /// Handles Google external login or registration.
        /// </summary>
        /// <remarks>
        /// This endpoint allows users to log in or register using their Google account.
        /// It accepts a Google ID token obtained from the frontend.
        /// </remarks>
        /// <param name="dto">Google login data containing the ID token.</param>
        [HttpPost("external-login/google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleExternalLogin([FromBody] GoogleLoginDto dto)
        {
            var result = await _authService.ExternalLoginGoogleAsync(dto.IdToken);
            if (!result.Success) return Unauthorized(result.Message);
            return Ok(new { token = result.Token, message = result.Message });
        }



        /// <summary>
        /// Completes a Google user's profile after initial login.
        /// </summary>
        /// <remarks>
        /// When a user registers via Google, only basic info is provided.
        /// Use this endpoint to complete missing data such as UserName,  gender, phone number, and national ID.
        /// </remarks>
        /// <param name="dto">Profile completion details.</param>
        [Authorize(Roles = "Customer")]
        [HttpPut("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileAfterGoogleDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _authService.CompleteProfileAsync(Guid.Parse(userId), dto);
            return result ? Ok("Profile updated successfully") : BadRequest("Profile update failed");
        }





        /// <summary>
        /// Logs in a user using email or UserName and password fro all users of APP.
        /// </summary>
        /// <remarks>
        /// This endpoint validates credentials and returns a JWT token containing user claims and roles.
        /// </remarks>
        /// <param name="dto">Login credentials (email and password).</param>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if(!result.Success ) 
            {
                return Unauthorized(result.Message);
            }
            return Ok(result); 
        }



        /// <summary>
        /// Confirms a Customer's email after registration.
        /// </summary>
        /// <remarks>
        /// The user clicks a link sent via email which calls this endpoint with their email and confirmation token.
        /// </remarks>
        /// <param name="email">The email address of the user (Customer).</param>
        /// <param name="token">The confirmation token sent by email.</param>
        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            var result =  await _authService.ConfirmEmailAsync(email, token);
            return Ok(result.Message);
        }



        /// <summary>
        /// Sends an OTP to the user’s email for password recovery.
        /// </summary>
        /// <remarks>
        /// This endpoint sent generated OTP code to user email with time expirary.
        /// </remarks>
        /// <param name="email">The email address of the user requesting password reset.</param>

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword( [FromBody] string email )
        {
            await _authService.ForgotPasswordAsync(email);
            return Ok(new { Message = "OTP sent to your email address." });
        }



        /// <summary>
        /// Verifies the OTP code sent to the user’s email.
        /// </summary>
        /// <remarks>
        /// This endpoint varify THE that sended to user mail and chick the time expiration.
        /// </remarks>
        /// <param name="dto">Contains the email and OTP code to verify.</param>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
           var result = await _authService.VerifyOtpAsync(dto.Email, dto.OtpCode);
            if(!result) return BadRequest("faild to verify The OTP code");
            return Ok(new { Message = "OTP verified successfully." });
        }


        /// <summary>
        /// Resets a user's password after successful OTP verification.
        /// </summary>
        /// <param name="dto">Contains email and new password information.</param>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto.Email, dto.NewPassword);
            return Ok(new { Message = "Password reset successfully." });
        }

    }


}
