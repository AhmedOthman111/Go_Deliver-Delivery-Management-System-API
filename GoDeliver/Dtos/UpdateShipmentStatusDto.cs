using GoDeliver.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoDeliver.Dtos
{
    public class UpdateShipmentStatusDto
    {
        [Required]
        public Guid ShipmentId { get; set; }

        [Required]
        public ShipmentStatus NewStatus { get; set; }
    }
}
