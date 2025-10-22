using GoDeliver.Application.Models.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface IExternalTokenValidator
    {
        Task<ExternalUserInfoDto> ValidateGoogleTokenAsync(string idToken);

    }
}
