using GoDeliver.Application.Models.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterCustomerAsync(CustomerRegisterDto dto);
        Task<AuthResult> RegisterEmployeeAsync(EmployeeRegisterDto dto);
        Task<AuthResult> RegisterRepresentativeAsync(RepresentativeRegisterDto dto);
        Task<AuthResult> LoginAsync(LoginDto dto);
        Task<AuthResult> ConfirmEmailAsync(string email, string token);

        // 🔑 Password recovery
        Task ForgotPasswordAsync(string email);
        Task<bool> VerifyOtpAsync(string usernameOrEmail, string otpCode);
        Task<AuthResult> ResetPasswordAsync(string email, string newPassword);
        Task<AuthResult> ExternalLoginGoogleAsync(string idToken);
        Task<bool> CompleteProfileAsync(Guid appUserId, CompleteProfileAfterGoogleDto dto);



    }
}
