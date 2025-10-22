using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Models.ShipmentService;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Repositories
{
    public class RepresentativeRepository : GenericRepository<Representative>, IRepresentativeRepository
    {
        public RepresentativeRepository(GoDeliverDbContext dbContext) : base(dbContext)
        {
        }


        public async Task<Representative> GetByAppUserIdAsync(Guid AppUserId)
        {
            var cus = await _dbSet.FirstOrDefaultAsync(c => c.AppUserId == AppUserId);
            if (cus == null) throw new NotFoundException("Representative", AppUserId);
            return cus;

        }

        public async  Task<IEnumerable<Representative>> GetAvelableRepresentativeInEmployeeGovernate(Guid employeeID)
        {
            var employee = await _Context.Employees.FirstOrDefaultAsync(e => e.AppUserId== employeeID);
            if (employee == null) throw new NotFoundException("Employee" , employeeID);

            var representatives = await _dbSet.Where(r => r.Governorate == employee.Governorate &&
                                                          r.Availability == RepresentativeAvailability.Available).ToListAsync();
            if (representatives.Count == 0) throw new ValidationException("there is No Avelable representatives that match employee governate");
            return representatives;
       
        }



    }
}
