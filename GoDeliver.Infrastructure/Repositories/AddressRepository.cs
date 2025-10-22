using GoDeliver.Core.Entities;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Repositories
{
    public class AddressRepository : GenericRepository<Address> , IAddressRepository
    {
        private readonly GoDeliverDbContext _context;

        public AddressRepository(GoDeliverDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
