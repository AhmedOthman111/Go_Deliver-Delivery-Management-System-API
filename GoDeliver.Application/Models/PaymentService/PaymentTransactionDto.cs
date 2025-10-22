using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.PaymentService
{
    public class PaymentTransactionDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public String Method { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }
    }
}
