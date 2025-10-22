
using GoDeliver.Application.Common.Models;
using GoDeliver.Infrastructure;
using GoDeliver.Infrastructure.DataSeeder;
using GoDeliver.Middleware;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace GoDeliver
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddControllers();



            builder.Services.Configure<ApiBehaviorOptions>(options => // Validation exception respose
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    var response = new ErrorResponse(400, "Validation Failed", errors);

                    return new BadRequestObjectResult(response);
                };
            });


            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.UseHangfireDashboard("/hangfire");

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await IdentitySeeder.SeedAsync(services);
                await PricingRuleSeeder.SeedAsync(services);

            }

            app.Run();
        }
    }
}
