using GoDeliver.Application.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(TokenUserModel user);
    }
}
