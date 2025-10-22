using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.ShipmentService
{
    public class ShipmentStatusHistoryDto
    {
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
    }
}
