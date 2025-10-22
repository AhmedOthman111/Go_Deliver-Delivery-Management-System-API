using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth
{
    public class TokenUserModel
    {
        public Guid Id { get; set; } 
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        public List<string> Roles { get; set; } = new();
    }
}
