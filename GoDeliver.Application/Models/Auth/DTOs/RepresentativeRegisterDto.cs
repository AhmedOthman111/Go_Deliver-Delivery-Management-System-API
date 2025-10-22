using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Auth.DTOs
{
    public class RepresentativeRegisterDto
    {

        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;


        [Required, StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits.")]
        public string NationalId { get; set; } = string.Empty;


        [Required, StringLength(50, MinimumLength = 4)]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone, StringLength(11, MinimumLength = 11, ErrorMessage = "phone number reqired 11 digits start with '01'")]
        public string PhoneNumber { get; set; } = string.Empty;


        [Required, StringLength(50)]
        public string Governorate { get; set; } = string.Empty;



        [Required, StringLength(20, MinimumLength = 5)]
        public string VehicleNumber { get; set; } = string.Empty;


        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;



        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "ConfirmPassword do not match Password.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
