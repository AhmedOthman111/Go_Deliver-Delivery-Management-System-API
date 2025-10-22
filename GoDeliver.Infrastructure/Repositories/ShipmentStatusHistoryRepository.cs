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
    public class ShipmentStatusHistoryRepository : GenericRepository<ShipmentStatusHistory>, IShipmentStatusHistoryRepository
    {
        public ShipmentStatusHistoryRepository(GoDeliverDbContext dbContext) : base(dbContext)
        {
        }
    }
}
