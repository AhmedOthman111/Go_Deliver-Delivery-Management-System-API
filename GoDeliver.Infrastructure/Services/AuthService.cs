using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.Auth;
using GoDeliver.Application.Models.Auth.DTOs;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Identity;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalTokenValidator _externalTokenValidator;
        private readonly string _otpSecretKey;


        public AuthService(UserManager<ApplicationUser> userManager , ITokenService tokenService 
                          , IConfiguration config, IEmailService emailService , IUnitOfWork unitOfWork 
                          , IExternalTokenValidator externalTokenValidator)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _config = config;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _externalTokenValidator = externalTokenValidator;
            _otpSecretKey = _config["Security:OtpSecretKey"]; 
        }


        //  Register Customer
        public async Task<AuthResult> RegisterCustomerAsync(CustomerRegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ValidationException("Email already registered before.");


            var existingUserByUsername = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUserByUsername != null)
                throw new ValidationException("Username already taken.");



            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,  
                FullName = dto.FullName,
                NationalID = dto.NationalId
            };
            
          await  using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new ValidationException(errorMessage);
                }

                await _userManager.AddToRoleAsync(user, "Customer");

                var customer = new Customer
                {
                    Gender = dto.Gender,
                    AppUserId = user.Id
                };

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsynic();

                var address = new Address
                {
                    Governorate = dto.Address.Governorate,
                    Street = dto.Address.Street,
                    City = dto.Address.City,
                    PostalCode = dto.Address.PostalCode,
                    District = dto.Address.District,
                    BuildingNumber = dto.Address.BuildingNumber,
                    CustomerId = customer.Id
                };
                await _unitOfWork.Address.AddAsync(address);
                await _unitOfWork.SaveChangesAsynic();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


            // Generate Email Confirmation Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmLink = $"{_config["App:BaseUrl"]}/api/auth/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

            // Send confirmation email (background via Hangfire)
            BackgroundJob.Enqueue(() =>
                _emailService.SendEmailAsync(user.Email, "Confirm Your Email",
                    $"Please confirm your email by clicking this link: <a href='{confirmLink}'>Confirm Email</a>" ,true ));

            return new AuthResult { Success = true, Message = "Registration successful. Please confirm your email." };
        }

        // Register Employee
        public async Task<AuthResult> RegisterEmployeeAsync(EmployeeRegisterDto dto)
        {
            var existing = await _userManager.FindByNameAsync(dto.UserName);
            if (existing != null)
                 throw new ValidationException("UserName already registered before.");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ValidationException("Email already registered before.");

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                NationalID = dto.NationalId,
                Email = dto.Email,
                EmailConfirmed = true
            };

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new ValidationException(errorMessage);
                }

                await _userManager.AddToRoleAsync(user, "Employee");

                var employee = new Employee
                {
                    Governorate = dto.Governorate,
                    AppUserId = user.Id,
                };

                await _unitOfWork.Employee.AddAsync(employee);
                await _unitOfWork.SaveChangesAsynic();

                await transaction.CommitAsync();

                return new AuthResult { Success = true, Message = "Employee registered successfully." };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //register Representative
        public async Task<AuthResult> RegisterRepresentativeAsync(RepresentativeRegisterDto dto)
        {
            var existing = await _userManager.FindByNameAsync(dto.UserName);
            if (existing != null)
                throw new ValidationException("UserName already registered before.");

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                NationalID = dto.NationalId,
                Email = dto.Email,
                EmailConfirmed = true

            };

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new ValidationException(errorMessage);
                }

                await _userManager.AddToRoleAsync(user, "Representative");

                var representative = new Representative
                {
                    Governorate = dto.Governorate,
                    VehicleNumber = dto.VehicleNumber,
                    AppUserId = user.Id,
                };

                await _unitOfWork.Representative.AddAsync(representative);
                await _unitOfWork.SaveChangesAsynic();
               
                await transaction.CommitAsync();

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new AuthResult { Success = true, Message = "Representative registered successfully." };
        }

        //login
        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            ApplicationUser user = null;

            // Try find by Email (Customer)
            user = await _userManager.FindByEmailAsync(dto.Identifier);
            if (user == null)
            {
                // Try find by Username (Employee, Representative, Admin)
                user = await _userManager.FindByNameAsync(dto.Identifier);
            }

            if (user == null)
                return new AuthResult { Success = false, Message = "Invalid credentials." };

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid)
                return new AuthResult { Success = false, Message = "Invalid credentials." };

            if (!user.EmailConfirmed)
            {
                var confermemailtoken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmLink = $"{_config["App:BaseUrl"]}/api/auth/confirm-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(confermemailtoken)}";


                BackgroundJob.Enqueue(() =>
                    _emailService.SendEmailAsync(user.Email, "Confirm Your Email",
                        $"Please confirm your email by clicking this link: <a href='{confirmLink}'>Confirm Email</a>", true));

                return new AuthResult { Success = true, Message = "Check your Bxo Mail to Conferm  your Account." };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokendto = new TokenUserModel
            {
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                UserName = user.UserName,
                Roles = roles.ToList(),
            };

            var token = await _tokenService.CreateTokenAsync(tokendto);

            return new AuthResult { Success = true, Token = token, Message = "Login successful." };
        }


        //Confirm Email
        public async Task<AuthResult> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            throw new NotFoundException("User" , email);


            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            throw new ValidationException("Invalid or expired token..");

            return new AuthResult { Success = true, Message = "Email confirmed successfully." };
        }


        // forget password and generate OTP code And send to mail
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("User", email);

            // Generate 6-digit OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            // Hash OTP before saving (never store raw)
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_otpSecretKey));
            var codeHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(otpCode)));


            user.CodeHashOtp = codeHash;
            user.OtpExpiryTime = DateTimeOffset.UtcNow.AddMinutes(5);
            user.OtpIsUsed = false;

            await _userManager.UpdateAsync(user);

            // Send OTP email (via background job)
            BackgroundJob.Enqueue(() =>
                _emailService.SendEmailAsync(
                    email,
                    "Password Reset OTP",
                    $"Your OTP code is <b>{otpCode}</b>. It will expire in 5 minutes.",
                    true
                )
            );
        }


        // recive OPT form user and check with that generated in DB and check expiry
        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_otpSecretKey));
            var codeHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(otp)));

            if (user.CodeHashOtp == null|| user.CodeHashOtp != codeHash 
                || user.OtpExpiryTime< DateTimeOffset.UtcNow || user.OtpIsUsed == true)
                return false;


            user.OtpIsUsed = true;
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<AuthResult> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("User", email);


            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new ValidationException(errorMessage);
            }

            return new AuthResult { Success = true, Message = "Password reset successfully." };
        }

        // google authentication
        public async Task<AuthResult> ExternalLoginGoogleAsync(string idToken)


        {
            var payload = await _externalTokenValidator.ValidateGoogleTokenAsync(idToken);


            if (! payload.IsEmailConfirmed)
                return new AuthResult { Success = false, Message = "Google account email not verified." };


            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    EmailConfirmed = true,
                    FullName = payload.FullName,
                    Provider = "Google",
                    ProviderKey = payload.Subject
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return new AuthResult { Success = false, Message = string.Join("; ", createResult.Errors.Select(e => e.Description)) };


                await _userManager.AddToRoleAsync(user, "Customer");

                var customer = new Customer
                {
                    AppUserId = user.Id,
                    Gender = "Unknown" 
                };
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsynic();
            }
            else
            {
                if (string.IsNullOrEmpty(user.Provider))
                {
                    user.Provider = "Google";
                    user.ProviderKey = payload.Subject;
                    await _userManager.UpdateAsync(user);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokendto = new TokenUserModel
            {
                Email = user.Email,
                FullName = user.FullName,
                Id = user.Id,
                UserName = user.UserName,
                Roles = roles.ToList(),
            };

            var token = await _tokenService.CreateTokenAsync(tokendto);

            return new AuthResult { Success = true, Token = token, Message = "External login successful." };
        }
        
        
        // complete profile if register with google 
        public async Task<bool> CompleteProfileAsync(Guid appUserId, CompleteProfileAfterGoogleDto dto)
        {
            var user = await _userManager.FindByIdAsync(appUserId.ToString());
            if (user == null) throw new NotFoundException("User " , appUserId);

            user.NationalID = dto.NationalId;
            user.PhoneNumber = dto.PhoneNumber;
            user.UserName = dto.UserName;

            var updateUser = await _userManager.UpdateAsync(user);
            if (!updateUser.Succeeded)
                throw new Exception(string.Join("; ", updateUser.Errors.Select(e => e.Description)));


            var customer = await _unitOfWork.Customers.GetByAppUserIdAsync(appUserId);

            if (customer != null)
            {
                customer.Gender = dto.Gender;
                _unitOfWork.Customers.Update(customer);
                await _unitOfWork.SaveChangesAsynic();
            }

            return true;
        }

    }
}
