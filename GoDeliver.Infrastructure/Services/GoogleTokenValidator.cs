using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.Auth.DTOs;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Services
{
    public class GoogleTokenValidator : IExternalTokenValidator
    {
        private readonly IConfiguration _config;
        public GoogleTokenValidator(IConfiguration config) { _config = config; }

        public async Task<ExternalUserInfoDto> ValidateGoogleTokenAsync(string idToken)
        {

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            if (payload == null) throw new ValidationException("Faild to valid google tokn");
            
            return new ExternalUserInfoDto
            {
                Subject = payload.Subject,
                Email = payload.Email,
                FullName = payload.Name,
                IsEmailConfirmed = payload.EmailVerified

            };
            

        }

    }
}
