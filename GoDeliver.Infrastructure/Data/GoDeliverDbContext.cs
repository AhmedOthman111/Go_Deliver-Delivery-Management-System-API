using GoDeliver.Core.Entities;
using GoDeliver.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Data
{
    public class GoDeliverDbContext :IdentityDbContext<ApplicationUser, IdentityRole<Guid> , Guid>
    {
        public GoDeliverDbContext( DbContextOptions<GoDeliverDbContext> options ) : base( options ) { }

        public  DbSet<Customer> Customers { get; set; } 
        public DbSet<Employee> Employees { get; set; } 
        public DbSet<Representative> Representatives { get; set; } 
        public DbSet<Address> Addresses { get; set; } 
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistory { get; set; } 
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } 
        public DbSet<PricingRule> PricingRules { get; set; } 
        public DbSet<EmailLog> EmailLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CUSTOMER
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasIndex(c => c.AppUserId).IsUnique();

                entity.HasOne<ApplicationUser>()
                      .WithOne()
                      .HasForeignKey<Customer>(c => c.AppUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Addresses)
                      .WithOne()
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // EMPLOYEE
            builder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AppUserId).IsUnique();

                entity.HasOne<ApplicationUser>()
                      .WithOne()
                      .HasForeignKey<Employee>(e => e.AppUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // REPRESENTATIVE
            builder.Entity<Representative>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.AppUserId).IsUnique();

                entity.HasOne<ApplicationUser>()
                      .WithOne()
                      .HasForeignKey<Representative>(r => r.AppUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ADDRESS
            builder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.HasOne<Customer>()
                      .WithMany(c => c.Addresses)
                      .HasForeignKey(a => a.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SHIPMENT
            builder.Entity<Shipment>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.TrackingNumber).IsUnique();

                // Sender & Recipient
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(s => s.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(s => s.RecipientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Assigned Representative (nullable)
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(s => s.AssignedRepresentativeId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Sender/Recipient Address
                entity.HasOne<Address>()
                      .WithMany()
                      .HasForeignKey(s => s.SenderAddressId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Address>()
                      .WithMany()
                      .HasForeignKey(s => s.RecipientAddressId)
                      .OnDelete(DeleteBehavior.Restrict);

                // PaymentTransaction 1:1 (nullable)
                entity.HasOne(s => s.PaymentTransaction)
                      .WithOne()
                      .HasForeignKey<Shipment>(s => s.PaymentTransactionId)
                      .OnDelete(DeleteBehavior.SetNull);


                entity.HasOne(s => s.PricingRule)
                      .WithMany(p => p.Shipments)
                      .HasForeignKey(s => s.PricingRuleId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Status History 1:M
                entity.HasMany(s => s.StatusHistory)
                      .WithOne()
                      .HasForeignKey(h => h.ShipmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SHIPMENT STATUS HISTORY
            builder.Entity<ShipmentStatusHistory>(entity =>
            {
                entity.HasKey(h => h.Id);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(h => h.ChangedByAppUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // PAYMENT TRANSACTION
            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasOne(p => p.Customer)
              .WithMany(c => c.PaymentTransactions)  
              .HasForeignKey(p => p.CustomerId);


            });

            // EMAIL LOG
            builder.Entity<EmailLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.AppUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne<Shipment>()
                      .WithMany()
                      .HasForeignKey(e => e.ShipmentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });


            builder.Entity<PricingRule>(entity =>
            {
                entity.HasKey(p => p.Id);
            });

        }



    }
}
