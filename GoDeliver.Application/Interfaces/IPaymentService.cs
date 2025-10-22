using GoDeliver.Application.Models.PaymentService;
using GoDeliver.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentTransaction> ProcessOnlinePaymentAsync(Guid CustomerID, decimal amount);
        Task<PaymentTransaction> RecordCashPaymentAsync(Guid CustomerID, decimal amount);
        Task<IEnumerable<PaymentTransactionDto>> GetCustomerPaymentsAsync (Guid CustomerID);
    }
}
