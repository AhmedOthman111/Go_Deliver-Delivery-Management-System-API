using GoDeliver.Application.Interfaces;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Configuration;
using GoDeliver.Infrastructure.Data;
using GoDeliver.Infrastructure.Identity;
using GoDeliver.Infrastructure.Repositories;
using GoDeliver.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure
{
    public static  class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure (this IServiceCollection services , IConfiguration configuration)
        {
            //  Database
            services.AddDbContext<GoDeliverDbContext>(options =>
                          options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


            //  Identity
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(
                options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireDigit = true;

                    options.SignIn.RequireConfirmedEmail = true;

                    options.User.RequireUniqueEmail = true;
                })
                    .AddEntityFrameworkStores<GoDeliverDbContext>()
                    .AddDefaultTokenProviders();


            // JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))

                    };

                });


            // confegeratio of SmtpSettings ( mail servcie)
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));




            //hangfire
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfireServer();




            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddScoped<IExternalTokenValidator, GoogleTokenValidator>();
            services.AddScoped<IPricingRuleService, PricingRuleService>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IRepresentativeRepository, RepresentativeRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IShipmentStatusHistoryRepository, ShipmentStatusHistoryRepository>();
            services.AddScoped<IPaymentTransactionRepository,PaymentTransactionRepository>();
            services.AddScoped<IPricingRuleRepository,PricingRuleRepository>();





            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;

        }
    }
}
