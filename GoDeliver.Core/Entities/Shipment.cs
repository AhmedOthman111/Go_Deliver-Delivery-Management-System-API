using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Entities
{
    public class Shipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TrackingNumber { get; set; } = string.Empty; // unique
        public Guid SenderId { get; set; }      // AppUser.Id
        public Guid RecipientId { get; set; }   // AppUser.Id
        public Guid SenderAddressId { get; set; }      // Address.Id
        public Guid RecipientAddressId { get; set; }   // Address.Id

        public decimal WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        public string PackageType { get; set; } = string.Empty; // fragile, doc, parcel
        public string ShipmentType { get; set; } = string.Empty; // internal/external (or derive)
        public WhoPays WhoPays { get; set; } = WhoPays.Sender;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        public Guid? AssignedRepresentativeId { get; set; } // AppUser.Id for representative (nullable)
        
        public Guid? PricingRuleId { get; set; }          
        public PricingRule? PricingRule { get; set; }       
        
        public Guid? PaymentTransactionId { get; set; }     // 1:1 link to PaymentTransaction
        public PaymentTransaction? PaymentTransaction { get; set; } // navigation

        public decimal Price { get; set; }
        public bool IsPaid { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }

        public Guid? CancelledById { get; set; }
        public string? CancelReason { get; set; }

        public ICollection<ShipmentStatusHistory> StatusHistory { get; set; } = new List<ShipmentStatusHistory>();
    }
}
