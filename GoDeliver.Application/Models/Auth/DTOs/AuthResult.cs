using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string ? Token { get; set; }
        public string ? Message { get; set; }
    }
}
