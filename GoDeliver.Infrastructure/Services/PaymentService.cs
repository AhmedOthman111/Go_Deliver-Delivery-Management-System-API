using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.PaymentService;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
          private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaymentTransaction> ProcessOnlinePaymentAsync(Guid CustomerID , decimal amount)
        {
            var transaction = new PaymentTransaction
            {
                CustomerId = CustomerID,
                Amount = amount,
                Method = PaymentMethod.Online,
                CreatedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow,
                Status = "Confirmed"
            };


            await _unitOfWork.PaymentTransaction.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsynic();

            return transaction;
        }

        public async Task<PaymentTransaction> RecordCashPaymentAsync(Guid CustomerID, decimal amount)
        {
            var transaction = new PaymentTransaction
            {
                CustomerId = CustomerID,
                Amount = amount,
                Method = PaymentMethod.Cash,
                Status = "Confirmed",
                CreatedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.PaymentTransaction.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsynic();

            return transaction;
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetCustomerPaymentsAsync (Guid appuserid)
        {
            var customer = await _unitOfWork.Customers.GetByAppUserIdAsync(appuserid);
            var payments = await _unitOfWork.PaymentTransaction.GetCustomerPaymentsByCustomerId( customer.Id);

            return payments.Select(p => new PaymentTransactionDto
            {
                Status = p.Status,
                Amount = p.Amount,
                Currency = p.Currency,  
                Method = p.Method.ToString(),
                CreatedAt =  p.CreatedAt,
                CompletedAt = p.CompletedAt
            });

        }
    }
}
