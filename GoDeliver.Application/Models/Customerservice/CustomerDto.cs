using GoDeliver.Application.Models.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.Customerservice
{
    public class CustomerDto
    {
        public string FullName { get; set; }
        public string NationalID { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public IEnumerable<GetAddressDto> Addresses { get; set; }
    }
}
