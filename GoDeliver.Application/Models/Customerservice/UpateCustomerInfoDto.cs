using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Customerservice
{
    public class UpateCustomerInfoDto
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;


        [Required, StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits.")]
        public string NationalId { get; set; } = string.Empty;


        [Required]
        [Phone, StringLength(11, MinimumLength = 11, ErrorMessage = "phone number reqired 14 digits start with '01'")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
