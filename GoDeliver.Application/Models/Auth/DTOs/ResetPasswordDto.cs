using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth.DTOs
{
    public class ResetPasswordDto
    {
        [Required , EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required , PasswordPropertyText]
        public string NewPassword { get; set; } = string.Empty;
    }
}
