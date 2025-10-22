using GoDeliver.Application.Exceptions;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Repositories
{
    public class PricingRuleRepository : GenericRepository<PricingRule>, IPricingRuleRepository
    {
        public PricingRuleRepository(GoDeliverDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PricingRule?> GetActiveRuleForWeightAndScopeAsync(decimal weightKg, PricingScope scope)
        {

            var rule = await _Context.PricingRules
                        .Where(r => r.IsActive
                                 && r.Scope == scope
                                 && (!r.MinWeightKg.HasValue || r.MinWeightKg <= weightKg)
                                 && (!r.MaxWeightKg.HasValue || r.MaxWeightKg >= weightKg))
                        .OrderBy(r => r.MinWeightKg)
                        .FirstOrDefaultAsync();
            if (rule == null) throw new ValidationException("there is no Matching craitera for Pricing Rule");

            return rule;

        }

    }
}
