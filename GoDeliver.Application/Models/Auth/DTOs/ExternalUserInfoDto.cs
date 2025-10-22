using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth.DTOs
{
    public class ExternalUserInfoDto
    {
        public string Email{ get; set;}

        public string FullName{ get; set;}  
        public string Subject { get; set;}  
        public bool IsEmailConfirmed { get; set;}   
    }
}
