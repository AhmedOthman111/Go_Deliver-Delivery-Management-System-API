using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.DataSeeder
{
    public class PricingRuleSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GoDeliverDbContext>();
           
            if (await context.PricingRules.AnyAsync())
                return;

            var rules = new List<PricingRule>
            {
                new PricingRule
                {
                    Name = "Light Internal",
                    Scope = PricingScope.Internal,
                    MinWeightKg = 1,
                    MaxWeightKg = 5,
                    BasePrice = 50,
                    PricePerKg = 8,
                    ExtraSizePrice = 10,
                    IsActive = true
                },
                new PricingRule
                {
                    Name = "Medium Internal",
                    Scope = PricingScope.Internal,
                    MinWeightKg = 6,
                    MaxWeightKg = 10,
                    BasePrice = 70,
                    PricePerKg = 5,
                    ExtraSizePrice = 25,
                    IsActive = true
                },
                new PricingRule
                {
                    Name = "Heavy Internal",
                    Scope = PricingScope.Internal,
                    MinWeightKg = 11,
                    MaxWeightKg = 50,
                    BasePrice = 100,
                    PricePerKg = 3,
                    ExtraSizePrice = 35,
                    IsActive = true
                },

                // 🔵 EXTERNAL RULES
                new PricingRule
                {
                    Name = "Light External",
                    Scope = PricingScope.External,
                    MinWeightKg = 1,
                    MaxWeightKg = 5,
                    BasePrice = 80,
                    PricePerKg = 12,
                    ExtraSizePrice = 25,
                    IsActive = true
                },
                new PricingRule
                {
                    Name = "Medium External",
                    Scope = PricingScope.External,
                    MinWeightKg = 6,
                    MaxWeightKg = 10,
                    BasePrice = 150,
                    PricePerKg = 10,
                    ExtraSizePrice = 35,
                    IsActive = true
                },
                new PricingRule
                {
                    Name = "Heavy External",
                    Scope = PricingScope.External,
                    MinWeightKg = 11,
                    MaxWeightKg = 50,
                    BasePrice = 220,
                    PricePerKg = 8,
                    ExtraSizePrice = 50,
                    IsActive = true
                }
            };

            await context.PricingRules.AddRangeAsync(rules);
            await context.SaveChangesAsync();

        }

    }
}
