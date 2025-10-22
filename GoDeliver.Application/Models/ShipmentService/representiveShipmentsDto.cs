using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public class representiveShipmentsDto
    {
        public Guid ShipmentId { get; set; } 
        public string TrackingNumber { get; set; } = string.Empty;
        public string ShipmentType { get; set; } = string.Empty;

        public string SenderName { get; set; }
        public Address senderaddress { get; set; }
        public string SenderPhonenum{ get; set; }
        
        public string RecipientName { get; set; }
        public Address Recipientaddress { get; set; }
        public string RecipientPhonenum { get; set; }

        public decimal WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        public string PackageType { get; set; } = string.Empty; 
        public decimal Price { get; set; }
        public bool IsPaid { get; set; }
        public WhoPays WhoPays { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
    }
}
