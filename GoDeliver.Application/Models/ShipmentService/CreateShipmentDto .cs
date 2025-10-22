using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.ShipmentService
{
    public class CreateShipmentDto
    {

        [Required , EmailAddress]
        public String Recipientemail { get; set; }   

        [Required]
        public Guid SenderAddressId { get; set; }
        [Required]
        public Guid RecipientAddressId { get; set; }
       
        [Required]
        public decimal WeightKg { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }

        [Required]
        public string PackageType { get; set; } = string.Empty;
        [Required]
        public string ShipmentType { get; set; } = string.Empty; // internal/external (or derive)

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public WhoPays WhoPays { get; set; } = WhoPays.Sender;
    }
}

