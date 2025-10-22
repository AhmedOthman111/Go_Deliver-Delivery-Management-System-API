using GoDeliver.Application.Exceptions;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Repositories
{
    public class ShipmentRepository : GenericRepository<Shipment>, IShipmentRepository
    {
        public ShipmentRepository(GoDeliverDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Shipment>> GetAllByCustomerIdAsync(Guid appUserId)
        {
     
              var shipments =  await  _dbSet.Where(s => s.SenderId == appUserId || s.RecipientId == appUserId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            if (shipments.Count() == 0) throw new NotFoundException("shippment not has sender or receptent", appUserId);
            return shipments;
        }

        public async Task<Shipment> GetByTrackingNumberAsync(string trackingNumber)
        {
            var shipment = await _dbSet
                .Include(s => s.StatusHistory.OrderBy(h => h.ChangedAt))
                .Include(s => s.PricingRule)
                .Include(s => s.PaymentTransaction)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
            if(shipment == null) throw new NotFoundException("Shipment", trackingNumber);

            return shipment;
        }

        public async Task<IEnumerable<Shipment>> GetCanceledShipments()
        {
            var shipments = await _dbSet.Where(s => s.Status == Core.Enums.ShipmentStatus.Cancelled).ToListAsync();
            if (shipments.Count == 0 ) throw new ValidationException("There is no canceled shipments");
            return shipments;
        }
        public async Task<IEnumerable<Shipment>> GetShipmentsThatAssignedTORepresemativeAsync(Guid repAppUserId)
        {
            var representative = await _Context.Representatives.FirstOrDefaultAsync(r => r.AppUserId == repAppUserId);

            if (representative == null)
                throw new NotFoundException("representative", repAppUserId);



            var shipments = await _dbSet.Where(s => s.AssignedRepresentativeId == repAppUserId &&
                                                s.Status != ShipmentStatus.Delivered && s.Status != ShipmentStatus.Cancelled).ToListAsync();
          
            if (shipments.Count == 0 )
                throw new ValidationException("No shipments assigned to representative  ");

            return shipments;
        }

        public async Task<IEnumerable<Shipment>> GetUnderReviewingShipmentsThatSenderAddresEQEmplyeeAddresAsync(Guid EmployeeID)
        {

            var employee = await _Context.Employees.FirstOrDefaultAsync(r => r.AppUserId == EmployeeID);

            if (employee == null)
                throw new NotFoundException("Employee", EmployeeID);

            var EmpGovernorate = employee.Governorate;

            var shipments = await (from s in _Context.Shipments
                                   join a in _Context.Addresses
                                       on s.SenderAddressId equals a.Id
                                   where a.Governorate == EmpGovernorate && s.Status == ShipmentStatus.UnderReview
                                   select s)
                                  .ToListAsync();
            return shipments;
        }

    }
}
