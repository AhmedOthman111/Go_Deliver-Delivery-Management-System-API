using GoDeliver.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
     private readonly GoDeliverDbContext _dbContext;
        public IEmployeeRepository Employee { get; }
        public ICustomerRepository Customers { get; }
        public IAddressRepository Address { get; }
        public IRepresentativeRepository Representative {  get; }
        public IShipmentRepository Shipment { get; }
        public IPaymentTransactionRepository PaymentTransaction { get; }
        public IShipmentStatusHistoryRepository ShipmentStatusHistory { get; }
        public IPricingRuleRepository PriceRule { get; }



        public UnitOfWork(GoDeliverDbContext dbContext , ICustomerRepository customers , IAddressRepository address , IEmployeeRepository employee,
            IRepresentativeRepository representative , IShipmentRepository shipment , IPaymentTransactionRepository paymentTransaction,
            IShipmentStatusHistoryRepository shipmentStatusHistory , IPricingRuleRepository pricingRule  )
        {
            Customers = customers;
            Address = address;
            Employee = employee;
            _dbContext = dbContext;
            Representative = representative;
            Shipment = shipment;
            PaymentTransaction = paymentTransaction;
            ShipmentStatusHistory = shipmentStatusHistory;
            PriceRule = pricingRule;
        }

        public async Task SaveChangesAsynic()
        {
            await _dbContext.SaveChangesAsync();
        }


        public async Task<IAppTransaction> BeginTransactionAsync()
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            return new EfTransaction(transaction);
        }
    }
}
