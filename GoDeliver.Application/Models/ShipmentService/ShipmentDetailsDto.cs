using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.ShipmentService
{
    public class ShipmentDetailsDto
    {
        public Guid Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;

        public string ShipmentType { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public string PackageType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsPaid { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }

        public List<ShipmentStatusHistoryDto> StatusHistory { get; set; } = new();
    }
}
