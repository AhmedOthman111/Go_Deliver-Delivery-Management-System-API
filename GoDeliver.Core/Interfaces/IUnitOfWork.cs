using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IAddressRepository   Address  { get; }
        IEmployeeRepository Employee { get; }
        IRepresentativeRepository Representative { get; }
        IShipmentRepository Shipment { get; }
        IPaymentTransactionRepository PaymentTransaction { get; }
        IShipmentStatusHistoryRepository ShipmentStatusHistory { get; }

        IPricingRuleRepository PriceRule { get; }

        Task<IAppTransaction> BeginTransactionAsync();

        Task SaveChangesAsynic();


    }

    public interface IAppTransaction : IAsyncDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
