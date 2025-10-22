using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.shared
{
    public class AddressDto
    {
        [Required, StringLength(30)]
        public string Governorate { get; set; } = string.Empty;


        [Required, StringLength(30)]
        public string City { get; set; } = string.Empty;


        [Required, StringLength(30)]
        public string District { get; set; } = string.Empty;


        [Required, StringLength(50)]
        public string Street { get; set; } = string.Empty;


        [Required, StringLength(10)]
        public string BuildingNumber { get; set; } = string.Empty;


        [StringLength(10)]
        public string PostalCode { get; set; } = string.Empty;
    }
}
