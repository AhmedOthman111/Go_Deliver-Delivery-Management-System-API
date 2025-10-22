using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.ShipmentService
{
    public class RepresentativeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string phoneNum { get; set; }
        public string VehicleNumber { get; set; } 
        public string Governorate { get; set; } 
        public string  Availability { get; set; } 
    }
}
