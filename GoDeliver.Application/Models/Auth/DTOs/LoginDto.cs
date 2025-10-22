using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth.DTOs
{
    public class LoginDto
    {
        public string Identifier { get; set; } // could be email or username
        public string Password { get; set; } = string.Empty;
    }
}
