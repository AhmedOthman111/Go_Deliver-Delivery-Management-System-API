using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Models.PricingRuleService
{
    public class PricingRuleDTO
    {

        public string Name { get; set; } = string.Empty;


        public PricingScope Scope { get; set; } 

        public decimal? MinWeightKg { get; set; }


        public decimal? MaxWeightKg { get; set; }


        public decimal BasePrice { get; set; }

        public decimal? PricePerKg { get; set; }

        public decimal? ExtraSizePrice { get; set; }

        public bool IsActive { get; set; }
    }
}
