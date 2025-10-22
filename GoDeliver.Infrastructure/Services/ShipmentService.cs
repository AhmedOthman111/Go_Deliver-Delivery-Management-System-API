using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.ShipmentService;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Identity;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IPaymentService _paymentService;

        public ShipmentService( IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,
                                IEmailService emailService, IPaymentService paymentService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailService = emailService;
            _paymentService = paymentService;
        }
        private async Task LogShipmentStatusChangeAsync(Guid shipmentId,ShipmentStatus oldStatus,ShipmentStatus newStatus, Guid? changedByUserId = null, string? note = null)
        {
            var history = new ShipmentStatusHistory
            {
                ShipmentId = shipmentId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedByAppUserId = changedByUserId,
                Note = note
            };

            await _unitOfWork.ShipmentStatusHistory.AddAsync(history);
            await _unitOfWork.SaveChangesAsynic();

        }

        private async Task HandleShipmentStatusEmailAsync(Shipment shipment, ShipmentStatus newStatus)
        {
            var sender = await _userManager.FindByIdAsync(shipment.SenderId.ToString());
            if(sender == null) throw new NotFoundException("sender App user" , shipment.SenderId);

            var recipient = await _userManager.FindByIdAsync(shipment.RecipientId.ToString());
            if (recipient == null) throw new NotFoundException("recipient App user", shipment.RecipientId);

            var representative = shipment.AssignedRepresentativeId != null
                ? await _userManager.FindByIdAsync(shipment.AssignedRepresentativeId.ToString()) : null;

            switch (newStatus)
            {
                case ShipmentStatus.RepOnWayToCollect:
                    if (sender != null && representative != null)
                    {
                        var subject = "Your Shipment is Being Collected";
                        var body = $@"
                    <h2>Representative On The Way to Collect Your Shipment</h2>
                    <p><strong>Representative:</strong> {representative.FullName}</p>
                    <p><strong>Phone:</strong> {representative.PhoneNumber}</p>
                    <p>Shipment Track Number: {shipment.TrackingNumber}</p>";

                        BackgroundJob.Enqueue(() =>
                         _emailService.SendEmailAsync(sender.Email, subject, body, true));
                    }
                    break;

                case ShipmentStatus.RepOnWayToDeliver:
                    if (recipient != null && representative != null)
                    {
                        var subject = "Your Shipment is On The Way";
                        var body = $@"
                    <h2>Representative On The Way to Deliver Your Shipment</h2>
                    <p><strong>Representative:</strong> {representative.FullName}</p>
                    <p><strong>Phone:</strong> {representative.PhoneNumber}</p>
                    <p>Shipment Track Number: {shipment.TrackingNumber}</p>";

                        BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(recipient.Email, subject, body, true));
                    }
                    break;

                case ShipmentStatus.Delivered:
                    var subjectDelivered = "Shipment Delivered Successfully";
                    var bodyDelivered = $@"
                <h2>Your Shipment Has Been Delivered</h2>
                <p>Shipment Track Number: {shipment.TrackingNumber}</p>
                <p>Status: {newStatus}</p>
                <p>Delivered by: {representative?.FullName ?? "Unknown"}</p>";

                    if (sender != null)
                        BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(sender.Email, subjectDelivered, bodyDelivered, true));

                    if (recipient != null)
                        BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(recipient.Email, subjectDelivered, bodyDelivered, true));
                    break;
            }
        }


        //---------------  Used in Customer controller ---------------


        // Create shipment - apply payments and create payments transaction - change shipment statue and create staue history-
        // handle sending mails to sender and Reciption -  used by sender(customer)
        public async Task<string> CreateShipmentAsync(Guid appUserId, CreateShipmentDto dto)
        {
            var senderappuser = await _userManager.FindByIdAsync(appUserId.ToString());
            if (senderappuser == null) throw new NotFoundException("customer(Sender)", appUserId);
            var senderCustomer = await _unitOfWork.Customers.GetByAppUserIdAsync(appUserId);

            var Recipientappuser = await _userManager.FindByEmailAsync(dto.Recipientemail);
            if (Recipientappuser == null) throw new NotFoundException("customer(Recipient)", dto.Recipientemail);


            var senderAddress = await _unitOfWork.Address.GetByIdAsync(dto.SenderAddressId);
            var recipientAddress = await _unitOfWork.Address.GetByIdAsync(dto.RecipientAddressId);
           

            var trackingNumber = $"TRK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";


            // Calculate price using Pricing Engine

            var scope = senderAddress.Governorate == recipientAddress.Governorate
                ? PricingScope.Internal
                : PricingScope.External;

            var pricingRule = await _unitOfWork.PriceRule.GetActiveRuleForWeightAndScopeAsync(dto.WeightKg, scope);

            
            decimal price = pricingRule.BasePrice;
            if (pricingRule.PricePerKg.HasValue)
                price += dto.WeightKg * pricingRule.PricePerKg.Value;

            var size = dto.LengthCm * dto.HeightCm * dto.WidthCm ;

            if (pricingRule.ExtraSizePrice.HasValue && size > 100)
                price += pricingRule.ExtraSizePrice.Value;



            var expecteddate = dto.ShipmentType == PricingScope.Internal.ToString() ? DateTimeOffset.UtcNow.AddDays(1) : DateTimeOffset.UtcNow.AddDays(2);
        
            
            var shipment = new Shipment
            {
                TrackingNumber = trackingNumber,
                SenderId = appUserId,
                RecipientId = Recipientappuser.Id,
                SenderAddressId = dto.SenderAddressId,
                RecipientAddressId = dto.RecipientAddressId,
                WeightKg = dto.WeightKg,
                LengthCm = dto.LengthCm,
                WidthCm = dto.WidthCm,
                HeightCm = dto.HeightCm,
                PackageType = dto.PackageType,
                ShipmentType = scope.ToString(),
                WhoPays = dto.WhoPays,
                PaymentMethod = dto.PaymentMethod,
                Status = ShipmentStatus.Created,
                PricingRuleId =pricingRule.Id,
                Price = price,
                ExpectedDeliveryDate= expecteddate
            };


            await _unitOfWork.Shipment.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsynic();

            if (dto.WhoPays == WhoPays.Sender)
            {
                if (dto.PaymentMethod == PaymentMethod.Online)
                {
                    var transaction = await _paymentService.ProcessOnlinePaymentAsync(senderCustomer.Id, price);
                    shipment.PaymentTransactionId = transaction.Id;
                    shipment.PaymentTransaction = transaction;
                    shipment.IsPaid = true;
                    

                    await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status , ShipmentStatus.UnderReview, senderappuser.Id, "Sender paid online.");

                    shipment.Status = ShipmentStatus.UnderReview;
                }
                else // Cash
                {

                    shipment.IsPaid = false;
;

                    await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.UnderReview, senderappuser.Id, "Sender chose cash payment.");

                    shipment.Status = ShipmentStatus.UnderReview; 
                }
            }
            else if (dto.WhoPays == WhoPays.Recipient)
            {


                await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.PendingRecipientApproval, senderappuser.Id, "Awaiting recipient approval.");
                shipment.Status = ShipmentStatus.PendingRecipientApproval;
            }

            await _unitOfWork.SaveChangesAsynic();


            string subject = $"New Shipment ({trackingNumber}) Created";
            string body = $@"
                            <h2>Shipment Created</h2>
                            <p><strong>Tracking Number:</strong> {trackingNumber}</p>
                            <p><strong>Sender:</strong> {senderappuser.FullName}</p>
                            <p><strong>Recipient City:</strong> {recipientAddress.City}</p>
                            <p><strong>Price:</strong> {price:C}</p>
                            <p><strong>Who Pays:</strong> {dto.WhoPays}</p>
                            <p><strong>Status:</strong> {shipment.Status}</p>";


            if (dto.WhoPays == WhoPays.Recipient)
            {
                string receptionistBody = $@"
                <h2>New Shipment Awaiting Approval</h2>
                <p>Tracking Number: <strong>{trackingNumber}</strong></p>
                <p>Sender: {senderappuser.FullName}</p>
                <p>Weight: {dto.WeightKg} kg</p>
                <p>Price to Pay: {price:C}</p>
                <p>Please review and accept the shipment in the Reception Dashboard.</p>";
                
                BackgroundJob.Enqueue(() =>
                  _emailService.SendEmailAsync(Recipientappuser.Email, "Shipment Awaiting Approval", receptionistBody, true));

                BackgroundJob.Enqueue(() =>
                    _emailService.SendEmailAsync(senderappuser.Email, subject, body, true));
            }

            else
            {
                BackgroundJob.Enqueue(() =>
                 _emailService.SendEmailAsync(senderappuser.Email, subject , body, true));
            }

            return trackingNumber;
        }


        // return Shipments that wating for Reciption appronal ther statue(PendingRecipientApproval)  - used by Recipient(cusmtomer)
        public async Task<IEnumerable<ShipmentDto>> GetWaitingReciptionApprovalShipmentsAsync(Guid ReciptionappUserId)
        {
            var pending = await _unitOfWork.Shipment.FindAsync(s => s.Status == ShipmentStatus.PendingRecipientApproval && s.RecipientId == ReciptionappUserId );
            var allUserIds = pending
                  .SelectMany(s => new[] { s.SenderId, s.RecipientId })
                  .Distinct()
                  .ToList();

            var users = await _userManager.Users
                .Where(u => allUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

            return pending.Select(s => new ShipmentDto
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,
                SenderName = userNames.GetValueOrDefault(s.SenderId, "Unknown"),
                RecipientName = userNames.GetValueOrDefault(s.RecipientId, "Unknown"),
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt,
                IsPaid = s.IsPaid,
                Price = s.Price,
                ShipmentType = s.ShipmentType,
                WhoPays = s.WhoPays.ToString(),
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
            });
        }


        //apply payments and create payments transaction - change shipment statue and create staue history-
        // and send mail to Reciption - used by Recipient(cusmtomer)
        public async Task AcceptShipmentAsync(Guid shipmentId, Guid recipientAppUserId , PaymentMethod paymentMethod)
        {
            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentId);

            if (shipment.Status != ShipmentStatus.PendingRecipientApproval)
                throw new ValidationException("This shipment is not awaiting receptionist approval.");

            var recipient = await _userManager.FindByIdAsync(recipientAppUserId.ToString());
            if (recipient == null) throw new NotFoundException("recipient", recipientAppUserId);
            var RecipientCustomer = await _unitOfWork.Customers.GetByAppUserIdAsync(recipientAppUserId);


            
            if (paymentMethod == PaymentMethod.Online)
            {
                 var transaction = await _paymentService.ProcessOnlinePaymentAsync(RecipientCustomer.Id, shipment.Price);
                shipment.PaymentTransactionId = transaction.Id;
                shipment.PaymentTransaction = transaction;
                shipment.IsPaid = true;
                await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.UnderReview, recipientAppUserId, "Recipient accepted and paid online.");

                shipment.Status = ShipmentStatus.UnderReview;
            }
            else
            {
                shipment.IsPaid = false;
                await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.UnderReview, recipientAppUserId, "Recipient accepted with cash payment.");

                shipment.Status = ShipmentStatus.UnderReview; 
            }

            _unitOfWork.Shipment.Update(shipment);
            await _unitOfWork.SaveChangesAsynic();

            BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(recipient.Email, "Shipment Accepted", $"Shipment {shipment.TrackingNumber} accepted successfully and the statue is {shipment.Status} .", true));

        }


        // cancel shipment by recipient - change statue to changed - used by Recipient(cusmtomer)
        public async Task RejectShipmentAsync(Guid shipmentId, Guid recipientAppUserId)
        {

            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentId);

            if (shipment.Status != ShipmentStatus.PendingRecipientApproval)
                throw new ValidationException("This shipment is not awaiting receptient approval.");

            var recipient = await _userManager.FindByIdAsync(recipientAppUserId.ToString());
            if (recipient == null) throw new NotFoundException("receptient", recipientAppUserId);

            shipment.CancelledById = recipient.Id;
            shipment.CancelReason = "Receptient Refuse to pay shipping cost";
            await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.Cancelled, recipientAppUserId, "receptient efuse to pay shipping cost ");
            shipment.Status = ShipmentStatus.Cancelled;

            _unitOfWork.Shipment.Update(shipment);
            await _unitOfWork.SaveChangesAsynic();
        }

       
        // return all shipments that  related to customer as (sender or receptient) - used by cusomter
        public async Task<IEnumerable<ShipmentDto>> GetAllShipmentsByCustomerAsync(Guid appUserId)
        {
            var shipments = await _unitOfWork.Shipment.GetAllByCustomerIdAsync(appUserId);

            var allUserIds = shipments
                .SelectMany(s => new[] { s.SenderId, s.RecipientId })
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => allUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

            var shipmentDtos = shipments.Select(s => new ShipmentDto
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,
                Status = s.Status.ToString(),
                Price = s.Price,
                CreatedAt = s.CreatedAt,
                SenderName = userNames.GetValueOrDefault(s.SenderId, "Unknown"),
                RecipientName = userNames.GetValueOrDefault(s.RecipientId, "Unknown"),
                IsPaid = s.IsPaid,
                WhoPays = s.WhoPays.ToString(),
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                ShipmentType = s.ShipmentType,
                CancelReason = s.Status == ShipmentStatus.Cancelled ? s.CancelReason : null


            }).ToList();

            return shipmentDtos;

        }


         // get shipment and his statue history by tarck number that related to customer as sender or recepient - used by customer
        public async Task<ShipmentDetailsDto> GetShipmentByTrackingAsync(string trackingNumber, Guid appUserId)
        {
            var shipment = await _unitOfWork.Shipment.GetByTrackingNumberAsync(trackingNumber);


            if (shipment.SenderId != appUserId && shipment.RecipientId != appUserId)
                throw new UnauthorizedAccessException("You do not have permission to view this shipment.");

            return new ShipmentDetailsDto
            {
                Id = shipment.Id,
                TrackingNumber = shipment.TrackingNumber,
                ShipmentType = shipment.ShipmentType,
                Status = shipment.Status.ToString(),
                WeightKg = shipment.WeightKg,
                PackageType = shipment.PackageType,
                Price = shipment.Price,
                IsPaid = shipment.IsPaid,
                CreatedAt = shipment.CreatedAt,
                ExpectedDeliveryDate = shipment.ExpectedDeliveryDate,
                StatusHistory = shipment.StatusHistory.Select(h => new ShipmentStatusHistoryDto
                {
                    OldStatus = h.OldStatus.ToString(),
                    NewStatus = h.NewStatus.ToString(),
                    Note = h.Note,
                    ChangedAt = h.ChangedAt
                }).ToList()
            };
        }


        // clancel shipement by change staue to canceled - crate statue history-  used by sender (customer)
        public async Task CancelShipmentAsync(Guid shipmentId, Guid appUserId)
        {
            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentId);

            if (shipment.SenderId != appUserId)
                throw new UnauthorizedAccessException("You cannot cancel this shipment.");

            if (shipment.IsPaid || shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Collected)
                throw new ValidationException("you can't cancel this shipment.");

            await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.Cancelled, appUserId, "sender cancel shipment");

            shipment.Status = ShipmentStatus.Cancelled;
            shipment.CancelledById = appUserId;
            shipment.CancelReason = "Canceled by sender";

            _unitOfWork.Shipment.Update(shipment);
            await _unitOfWork.SaveChangesAsynic();
        }



        //  EMPLOYEE / ADMIN METHODS



        // return shipment with statue history by track number -  used by employee or admin
        public async Task<ShipmentDetailsDto> SearchAboutShipmentByTrackNumber(string trackingNumber)
        {
            var shipment = await _unitOfWork.Shipment.GetByTrackingNumberAsync(trackingNumber);


            return new ShipmentDetailsDto
            {
                Id = shipment.Id,
                TrackingNumber = shipment.TrackingNumber,
                ShipmentType = shipment.ShipmentType,
                Status = shipment.Status.ToString(),
                WeightKg = shipment.WeightKg,
                PackageType = shipment.PackageType,
                Price = shipment.Price,
                IsPaid = shipment.IsPaid,
                CreatedAt = shipment.CreatedAt,
                ExpectedDeliveryDate = shipment.ExpectedDeliveryDate,
                StatusHistory = shipment.StatusHistory.Select(h => new ShipmentStatusHistoryDto
                {
                    OldStatus = h.OldStatus.ToString(),
                    NewStatus = h.NewStatus.ToString(),
                    Note = h.Note,
                    ChangedAt = h.ChangedAt
                }).ToList()
            };
        }
       

        // return all shimpents - used by admin
        public async Task<IEnumerable<ShipmentDto>> GetAllShipmentsAsync()
        {
            var shipments = await _unitOfWork.Shipment.GetAllAsync();
            if (!shipments.Any())
                throw new ValidationException("shipments");

            var allUserIds = shipments
                .SelectMany(s => new[] { s.SenderId, s.RecipientId })
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => allUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

            return shipments.Select(s => new ShipmentDto
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,
                SenderName = userNames.GetValueOrDefault(s.SenderId, "Unknown"),
                RecipientName = userNames.GetValueOrDefault(s.RecipientId, "Unknown"),
                Status = s.Status.ToString(),
                ShipmentType = s.ShipmentType,
                WhoPays = s.WhoPays.ToString(),
                Price = s.Price,
                IsPaid = s.IsPaid,
                CreatedAt = s.CreatedAt,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
            });
        }

    

        // assin shipmet to representative that meet the sender shipmet governate - change and crate statur history - used by employee
        public async Task AssignRepresentativeAsync(Guid shipmentId, Guid representativeAppUserId)
        {
            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentId);
            
            var senderaddres = await _unitOfWork.Address.GetByIdAsync(shipment.SenderAddressId);


            var rep = await _unitOfWork.Representative.GetByAppUserIdAsync(representativeAppUserId);

            if (rep.Availability != RepresentativeAvailability.Available)
                throw new ValidationException("Representative is not available.");
           
            if (rep.Governorate != senderaddres.Governorate )
                throw new ValidationException(" shipment is out of Representative area .");

            shipment.AssignedRepresentativeId = representativeAppUserId;
            await LogShipmentStatusChangeAsync(shipment.Id, shipment.Status, ShipmentStatus.RepAssigned, note: "shipment assigned to representative.");

            shipment.Status = ShipmentStatus.RepAssigned;
            _unitOfWork.Shipment.Update(shipment);
            await _unitOfWork.SaveChangesAsynic();
        }



        //get avelable representitves  that match employee governrate - used by employee
        public async Task<IEnumerable<RepresentativeDto>> GetAvelableRepresentativeInEmployeeGovernate(Guid employeeID)
        {
           var representatives = await _unitOfWork.Representative.GetAvelableRepresentativeInEmployeeGovernate(employeeID);

            var representativeAppUserIds = representatives
                 .Select(r => r.AppUserId)
                 .Distinct()
                 .ToList();
            var representativeUsers = await _userManager.Users
                 .Where(u => representativeAppUserIds.Contains(u.Id))
                 .Select(u => new
                 {
                     u.Id,
                     u.FullName,
                     u.PhoneNumber
                 })
                 .ToListAsync();

            var userDictionary = representativeUsers.ToDictionary(u => u.Id, u => new
            {
                u.FullName,
                u.PhoneNumber
            });

            return representatives.Select(x => new RepresentativeDto 
            {
               Id = x.AppUserId,
                VehicleNumber = x.VehicleNumber,
                Availability = x.Availability.ToString(),
                Governorate = x.Governorate,
                Name = userDictionary.GetValueOrDefault(x.AppUserId)?.FullName ?? "N/A",
                phoneNum = userDictionary.GetValueOrDefault(x.AppUserId)?.PhoneNumber ?? "N/A"
            });
        }



        // get under review shipments that match employee governate - used by employee
        public async Task<IEnumerable<ShipmentDto>> GetUnderReviewingShipmentsAsync(Guid EmployeeID)
        {
          
            var underreviewshipments = await _unitOfWork.Shipment.GetUnderReviewingShipmentsThatSenderAddresEQEmplyeeAddresAsync(EmployeeID);
            if (!underreviewshipments.Any()) throw new NotFoundException("Shipments with statue under review and sender Governate equal to employee governate ", EmployeeID);

            var allUserIds = underreviewshipments
                .SelectMany(s => new[] { s.SenderId, s.RecipientId })
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => allUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();
            var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

            return underreviewshipments.Select(s => new ShipmentDto
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,
                SenderName = userNames.GetValueOrDefault(s.SenderId, "Unknown"),
                RecipientName = userNames.GetValueOrDefault(s.RecipientId, "Unknown"),
                Status = s.Status.ToString(),
                ShipmentType =s.ShipmentType,
                CreatedAt = s.CreatedAt,
                WhoPays = s.WhoPays.ToString(),
                IsPaid = s.IsPaid,
                Price = s.Price,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,

            });
        }


        // get all shipments that canceled - used by admin
        public async Task<IEnumerable<ShipmentDto>> GetCanceledShipments()
        {
            var shipments = await _unitOfWork.Shipment.GetCanceledShipments();


            var allUserIds = shipments
                .SelectMany(s => new[] { s.SenderId, s.RecipientId })
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => allUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToListAsync();

            var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

            return shipments.Select(s => new ShipmentDto
            {
                Id = s.Id,
                TrackingNumber = s.TrackingNumber,
                SenderName = userNames.GetValueOrDefault(s.SenderId, "Unknown"),
                RecipientName = userNames.GetValueOrDefault(s.RecipientId, "Unknown"),
                ShipmentType = s.ShipmentType,
                Status = s.Status.ToString(),
                WhoPays = s.WhoPays.ToString() ,
                Price = s.Price,
                IsPaid = s.IsPaid,
                CreatedAt = s.CreatedAt,
                CancelReason = s.CancelReason,
            });

        }



        // change statue of shipments  by any one
        public async Task UpdateShipmentStatusAsync(Guid userid, Guid shipmentId, ShipmentStatus newstatus, string notes)
        {
            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentId);

            var oldStatus = shipment.Status;
            shipment.Status = newstatus;

            await LogShipmentStatusChangeAsync(shipment.Id, oldStatus, newstatus, userid, notes);

            _unitOfWork.Shipment.Update(shipment);
            await _unitOfWork.SaveChangesAsynic();
            await HandleShipmentStatusEmailAsync(shipment, newstatus);

        }




        //represetative

        // use when represenatve select the shipment  (from repassigned to RepOnWayToCollect)
        //after collected ( from RepOnWayToCollect  to collected )
        // when he go to delevever (from collected  to RepOnWayToDeliver )
        // when dliver the shipmet (from RepOnWayToDeliver  to Delivered )
        //used by  representative only
        public async Task UpdateMyShipmentStatusAsync(Guid repAppUserId, Guid shipmentId, ShipmentStatus newStatus, string API , string? notes = null )
        {
            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentId);

            if (shipment.AssignedRepresentativeId != repAppUserId)
                throw new UnauthorizedAccessException("You are not authorized to update this shipment.");

            if (API == "select-shipment" && shipment.Status != ShipmentStatus.RepAssigned)
                throw new ValidationException("it's Not normal flow of shipment statue");
            
            if(API == "shipment-collected" && shipment.Status != ShipmentStatus.RepOnWayToCollect)
                throw new ValidationException("it's Not normal flow of shipment statue");

            if (API == "delivering-shipment" && shipment.Status != ShipmentStatus.Collected)
                throw new ValidationException("it's Not normal flow of shipment statue");

            if (API == "shipment-delived" && shipment.Status != ShipmentStatus.RepOnWayToDeliver)
                throw new ValidationException("it's Not normal flow of shipment statue");

            await UpdateShipmentStatusAsync(repAppUserId, shipmentId, newStatus, notes ?? "Updated by representative");
        }
        
        
        public async Task Collectmoney (Guid shipmentid , string API)
        {
            var shipment = await _unitOfWork.Shipment.GetByIdAsync(shipmentid);


            if (!shipment.IsPaid)
            {
                if(shipment.WhoPays == WhoPays.Sender && shipment.PaymentMethod == PaymentMethod.Cash && API == "shipment-collected")
                {
                    var senderappuser = await _userManager.FindByIdAsync(shipment.SenderId.ToString());
                    if (senderappuser == null) throw new NotFoundException("customer(Sender)", shipment.SenderId);
                    var senderCustomer = await _unitOfWork.Customers.GetByAppUserIdAsync(senderappuser.Id);


                    var transaction = await _paymentService.RecordCashPaymentAsync(senderCustomer.Id, shipment.Price);
                    shipment.PaymentTransactionId = transaction.Id;
                    shipment.PaymentTransaction = transaction;
                    shipment.IsPaid = true;
                }
                else if (shipment.WhoPays == WhoPays.Recipient && shipment.PaymentMethod == PaymentMethod.Cash && API == "shipment-delivered")
                {
                    var Recipientappuser = await _userManager.FindByIdAsync(shipment.RecipientId.ToString());
                    if (Recipientappuser == null) throw new NotFoundException("customer(Recipient)", shipment.RecipientId);
                    var ReceptiontCustomer = await _unitOfWork.Customers.GetByAppUserIdAsync(Recipientappuser.Id);

                    var transaction = await _paymentService.RecordCashPaymentAsync(ReceptiontCustomer.Id, shipment.Price);
                    shipment.PaymentTransactionId = transaction.Id;
                    shipment.PaymentTransaction = transaction;
                    shipment.IsPaid = true;

                }
                _unitOfWork.Shipment.Update(shipment);
                await _unitOfWork.SaveChangesAsynic();
            }

        }




        // change represntative Availability from ondelivery to avelable  after dlevir the shipment - used by representative
        public async Task ChangeRepresenatativeStatueToAvalable(Guid RepId)
        {
            var representative = await _unitOfWork.Representative.GetByAppUserIdAsync(RepId);
            representative.Availability = RepresentativeAvailability.Available;

            _unitOfWork.Representative.Update(representative);
            await _unitOfWork.SaveChangesAsynic();
        }


        //  change represntative Availability from avelable  to ondelivery  after select shipment to be on his way to collect  - used by representative
        public async Task ChangeRepresenatativeStatueToOnDlivery(Guid RepId)
        {
            var representative = await _unitOfWork.Representative.GetByAppUserIdAsync(RepId);
            representative.Availability = RepresentativeAvailability.OnDelivery;

            _unitOfWork.Representative.Update(representative);
            await _unitOfWork.SaveChangesAsynic();
        }

        
        // return the shipments that assigined to representative - used by representative
        public async Task<IEnumerable<representiveShipmentsDto>> GetMyShipmentsAsync(Guid repAppUserId)
        {

            var shipments = await _unitOfWork.Shipment.GetShipmentsThatAssignedTORepresemativeAsync(repAppUserId);
          
            var allUserIds = shipments
                .SelectMany(s => new[] { s.SenderId, s.RecipientId })
                .Distinct()
                .ToList();

            var allAddressIds = shipments
                .SelectMany(s => new[] { s.SenderAddressId, s.RecipientAddressId })
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => allUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.PhoneNumber })
                .ToListAsync();

            var userData = users.ToDictionary(u => u.Id, u => new { u.FullName, u.PhoneNumber });


            var addresses = await _unitOfWork.Address
                .FindAsync(a => allAddressIds.Contains(a.Id));

            var addressData = addresses.ToDictionary(a => a.Id, a => a);


            var sh = shipments.Select(s => new representiveShipmentsDto
            {
                ShipmentId = s.Id,   
                TrackingNumber = s.TrackingNumber,
                ShipmentType = s.ShipmentType,
              
                SenderName = userData.GetValueOrDefault(s.SenderId)?.FullName ?? "Unknown",
                SenderPhonenum = userData.GetValueOrDefault(s.SenderId)?.PhoneNumber ?? "N/A",
                senderaddress = addressData.GetValueOrDefault(s.SenderAddressId), 

                RecipientName = userData.GetValueOrDefault(s.RecipientId)?.FullName ?? "Unknown",
                RecipientPhonenum = userData.GetValueOrDefault(s.RecipientId)?.PhoneNumber ?? "N/A",
                Recipientaddress = addressData.GetValueOrDefault(s.RecipientAddressId), 

                PackageType = s.PackageType,
                WeightKg = s.WeightKg,
                LengthCm = s.LengthCm,
                HeightCm = s.HeightCm,
                WidthCm = s.WidthCm,
                Price = s.Price,
                IsPaid = s.IsPaid,
                WhoPays = s.WhoPays,
                ExpectedDeliveryDate = s.ExpectedDeliveryDate,
            }).ToList();

            return sh;
        }



    }
}
