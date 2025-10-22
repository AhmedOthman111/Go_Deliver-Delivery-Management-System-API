using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Models.PaymentService;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(GoDeliverDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<PaymentTransaction>> GetCustomerPaymentsByCustomerId(Guid customerId)
        {
            var payments = await _dbSet.Where(p => p.CustomerId == customerId).ToListAsync();
            if (payments.Count() == 0) throw new NotFoundException("Payments for this customer " , customerId);
            return payments;
        }
    }
}
