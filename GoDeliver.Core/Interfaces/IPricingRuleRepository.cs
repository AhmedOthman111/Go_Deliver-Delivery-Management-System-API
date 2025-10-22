using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public interface IPricingRuleRepository : IGenericRepository<PricingRule>
    {
        Task<PricingRule?> GetActiveRuleForWeightAndScopeAsync(decimal weightKg , PricingScope scope);
    }
}
