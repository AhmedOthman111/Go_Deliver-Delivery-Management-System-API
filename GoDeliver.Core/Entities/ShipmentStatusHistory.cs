using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Entities
{
    public class ShipmentStatusHistory
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ShipmentId { get; set; }
        public ShipmentStatus OldStatus { get; set; }
        public ShipmentStatus NewStatus { get; set; }
        public Guid? ChangedByAppUserId { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
