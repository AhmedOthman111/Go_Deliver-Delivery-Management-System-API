using GoDeliver.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {
        Task<IEnumerable<PaymentTransaction>> GetCustomerPaymentsByCustomerId(Guid customerId);
    }
}
