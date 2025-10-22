using GoDeliver.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public  interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetCustomerWithAddressesAsync(Guid AppUserId);
        Task<Customer>GetByAppUserIdAsync(Guid AppUserId);
        Task<IEnumerable<Customer>> GetAllWithAddressesAsync();

    }
}
