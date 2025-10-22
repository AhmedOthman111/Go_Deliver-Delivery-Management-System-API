using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.PricingRuleService
{
    public class CreatePricingDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public PricingScope Scope { get; set; } = PricingScope.Internal;

        [Required]
        public decimal? MinWeightKg { get; set; }

        [Required]
        public decimal? MaxWeightKg { get; set; }

        [Required]
        public decimal BasePrice { get; set; }
        [Required]
        public decimal? PricePerKg { get; set; }

        public decimal? ExtraSizePrice { get; set; }
        
        [Required]
        public bool IsActive { get; set; } = true;
    }
}
