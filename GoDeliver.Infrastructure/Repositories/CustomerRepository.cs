using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Models.Customerservice;
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
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {

        private readonly GoDeliverDbContext _context;

        public CustomerRepository(GoDeliverDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerWithAddressesAsync(Guid AppUserId)
        {
            var CustomerWithAddresses = await _context.Customers.Include(x => x.Addresses)
                                                      .FirstOrDefaultAsync(c => c.AppUserId == AppUserId);
            if (CustomerWithAddresses == null) throw new NotFoundException("Customer", AppUserId);
            return CustomerWithAddresses;
        }


        public async Task<Customer> GetByAppUserIdAsync(Guid AppUserId)
        {
            var cus  = await _Context.Customers.FirstOrDefaultAsync(c => c.AppUserId == AppUserId);
            if (cus == null) throw new NotFoundException("Customer", AppUserId);
            return cus;

        }


        public async Task<IEnumerable<Customer>> GetAllWithAddressesAsync()
        {
            var Customers =  await _context.Customers
                                            .Include(c => c.Addresses)
                                            .ToListAsync();
            if (Customers.Count == 0) throw new ValidationException("There is no customers with addresses");
            return Customers;
        }

    }
}
