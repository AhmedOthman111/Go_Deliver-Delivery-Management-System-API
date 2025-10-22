using GoDeliver.Application.Models.PricingRuleService;
using GoDeliver.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface IPricingRuleService
    {
        Task<IEnumerable<PricingRuleDTO>> GetAllPricingRulesAsync();
        Task<PricingRule> CreateAsync(CreatePricingDto rule);
        Task<PricingRule> UpdateAsync(Guid PricingRuleid, CreatePricingDto updatedRule);
        Task DeleteAsync(Guid id);

    }
}
