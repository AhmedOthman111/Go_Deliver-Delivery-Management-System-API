using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.PricingRuleService;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Services
{
    public class PricingRuleService : IPricingRuleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PricingRuleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<IEnumerable<PricingRuleDTO>> GetAllPricingRulesAsync()
        {
            var rules =  await _unitOfWork.PriceRule.GetAllAsync();
            return rules.Select(r => new PricingRuleDTO
            {
                Name = r.Name,
                Scope = r.Scope,
                MinWeightKg = r.MinWeightKg,
                MaxWeightKg = r.MaxWeightKg,
                BasePrice = r.BasePrice,
                PricePerKg = r.PricePerKg,
                ExtraSizePrice = r.ExtraSizePrice,
                IsActive = r.IsActive,
            });
        }

        public async Task<PricingRule> CreateAsync(CreatePricingDto rule)
        {
            var priceRule = new PricingRule
            {
                PricePerKg = rule.PricePerKg,
                MaxWeightKg = rule.MaxWeightKg,
                MinWeightKg = rule.MinWeightKg,
                ExtraSizePrice = rule.ExtraSizePrice,
                BasePrice = rule.BasePrice, 
                Scope = rule.Scope,
                Name = rule.Name,
                IsActive = rule.IsActive,
            };
            await _unitOfWork.PriceRule.AddAsync(priceRule);
            await _unitOfWork.SaveChangesAsynic();
            return priceRule;
        }

        public async Task<PricingRule> UpdateAsync(Guid PricingRuleid, CreatePricingDto updatedRule)
        {
            var existing = await _unitOfWork.PriceRule.GetByIdAsync(PricingRuleid);

            existing.Name = updatedRule.Name;
            existing.MinWeightKg = updatedRule.MinWeightKg;
            existing.MaxWeightKg = updatedRule.MaxWeightKg;
            existing.BasePrice = updatedRule.BasePrice;
            existing.PricePerKg = updatedRule.PricePerKg;
            existing.ExtraSizePrice = updatedRule.ExtraSizePrice;
            existing.IsActive = updatedRule.IsActive;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.IsActive = updatedRule.IsActive;

            _unitOfWork.PriceRule.Update(existing);
            await _unitOfWork.SaveChangesAsynic();
            return existing;
        }

        public async Task DeleteAsync(Guid id)
        {
            var rule = await _unitOfWork.PriceRule.GetByIdAsync(id);

            _unitOfWork.PriceRule.Delete(rule);
            await _unitOfWork.SaveChangesAsynic();
        }


    }
}
