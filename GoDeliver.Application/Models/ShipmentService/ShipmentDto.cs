using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.ShipmentService
{
    public class ShipmentDto
    {
        public Guid Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string? SenderName{ get; set; }
        public string? RecipientName { get; set; }

        public String ShipmentType { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsPaid { get; set; }
        public string WhoPays { get; set; } 
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public string? CancelReason { get; set; }
    }
}
