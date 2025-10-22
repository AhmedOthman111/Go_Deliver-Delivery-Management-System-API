using System.ComponentModel.DataAnnotations;

namespace GoDeliver.Dtos
{
    public class AssignRepresentativeDto
    {
        [Required]
        public Guid ShipmentId { get; set; }

        [Required]
        public Guid RepresentativeId { get; set; }
    }
}
