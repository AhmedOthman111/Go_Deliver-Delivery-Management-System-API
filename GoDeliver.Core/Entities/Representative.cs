using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Entities
{
    public class Representative
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppUserId { get; set; }   
        public string VehicleNumber { get; set; } = string.Empty;
        public string Governorate { get; set; } = string.Empty;
        public RepresentativeAvailability Availability { get; set; } = RepresentativeAvailability.Available;
    }
}

