using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth.DTOs
{
    public class CompleteProfileAfterGoogleDto
    {

        [Required, RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
        public string Gender { get; set; } = string.Empty;


        [Required, StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits.")]
        public string NationalId { get; set; } = string.Empty;


        [Required, StringLength(50, MinimumLength = 4)]
        public string UserName { get; set; } = string.Empty;


        [Required]
        [Phone, StringLength(11, MinimumLength = 11, ErrorMessage = "phone number reqired 14 digits start with '01'")]
        public string PhoneNumber { get; set; } = string.Empty;

    }
}
