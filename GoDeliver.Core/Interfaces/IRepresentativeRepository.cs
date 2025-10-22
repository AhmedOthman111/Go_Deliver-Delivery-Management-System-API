using GoDeliver.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Core.Interfaces
{
    public interface IRepresentativeRepository : IGenericRepository<Representative>
    {
        Task<Representative> GetByAppUserIdAsync(Guid AppUserId);
        Task<IEnumerable<Representative>> GetAvelableRepresentativeInEmployeeGovernate(Guid employeeID);
    }
}
