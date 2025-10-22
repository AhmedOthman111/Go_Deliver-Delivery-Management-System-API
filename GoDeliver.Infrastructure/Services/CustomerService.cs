using GoDeliver.Application.Exceptions;
using GoDeliver.Application.Interfaces;
using GoDeliver.Application.Models.Customerservice;
using GoDeliver.Application.Models.shared;
using GoDeliver.Core.Entities;
using GoDeliver.Core.Enums;
using GoDeliver.Core.Interfaces;
using GoDeliver.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }


        // return customer info with his addresses
        public async Task<CustomerDto> GetProfileAsync(Guid appUserId)
        {
            var CustomerWithAddress = await _unitOfWork.Customers.GetCustomerWithAddressesAsync(appUserId);

            var appuser = await _userManager.FindByIdAsync(appUserId.ToString());

            return new CustomerDto
            {
                FullName = appuser.FullName,
                NationalID = appuser.NationalID,
                Gender = CustomerWithAddress.Gender,
                UserName = appuser.UserName,
                Email = appuser.Email,
                PhoneNumber = appuser.PhoneNumber,
                Addresses = CustomerWithAddress.Addresses.Select(
                    a => new GetAddressDto
                    {
                        Id = a.Id,
                        PostalCode = a.PostalCode,
                        Governorate = a.Governorate,
                        City = a.City,
                        District = a.District,
                        Street = a.Street,
                        BuildingNumber = a.BuildingNumber,
                    })

            };

        }


        //get addres info and add it
        public async Task AddAddressAsync(Guid appUserId, AddressDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByAppUserIdAsync(appUserId);

            var address = new Address
            {
                CustomerId = customer.Id,
                PostalCode = dto.PostalCode,
                Governorate = dto.Governorate,
                City = dto.City,
                District = dto.District,
                Street = dto.Street,
                BuildingNumber = dto.BuildingNumber,
            };

            await _unitOfWork.Address.AddAsync(address);
            await _unitOfWork.SaveChangesAsynic();

        }


        public async Task<IEnumerable<GetAddressDto>> GetAddressesAsync(Guid appUserId)
        {
            var customer = await _unitOfWork.Customers.GetByAppUserIdAsync(appUserId);


            var addresses = await _unitOfWork.Address.FindAsync(a => a.CustomerId == customer.Id);

            return addresses.Select(a => new GetAddressDto
            {
                Id = a.Id,
                Governorate = a.Governorate,
                City = a.City,
                District = a.District,
                Street = a.Street,
                BuildingNumber = a.BuildingNumber,
                PostalCode = a.PostalCode
            });


        }

        //update Customer info
        public async Task UpdateCustomerinfoAsync(Guid appUserId, UpateCustomerInfoDto dto)
        {
            var appuser = await _userManager.FindByIdAsync(appUserId.ToString());
            if (appuser == null) throw new NotFoundException("customer", appUserId);

            appuser.PhoneNumber = dto.PhoneNumber;
            appuser.FullName = dto.FullName;
            appuser.NationalID = dto.NationalId;
            
            await _userManager.UpdateAsync(appuser);

        }


        // update address
        public async Task<AddressDto> UpdateAddressAsync(Guid appUserId, Guid addressId, AddressDto dto)
        {
            var customer = await _unitOfWork.Customers.GetByAppUserIdAsync(appUserId);
           
            var address = await _unitOfWork.Address.GetByIdAsync(addressId);

            if (customer.Id != address.CustomerId) throw new ValidationException("Not allowed to update");

            address.Governorate = dto.Governorate;
            address.City = dto.City;
            address.District = dto.District;
            address.Street = dto.Street;
            address.BuildingNumber = dto.BuildingNumber;
            address.PostalCode = dto.PostalCode;

            _unitOfWork.Address.Update(address);
            await _unitOfWork.SaveChangesAsynic();

            return dto;
        }

        //delete  specif address of customer 
        public async Task DeleteAddressAsync(Guid appUserId, Guid addressId)
        {
            var customer = await _unitOfWork.Customers.GetByAppUserIdAsync(appUserId);


            var address = await _unitOfWork.Address.GetByIdAsync(addressId);


            if (address == null || address.CustomerId != customer.Id)
                throw new NotFoundException("Address", addressId);

            _unitOfWork.Address.Delete(address);
            await _unitOfWork.SaveChangesAsynic();
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomers()
        {
            var customers = await _unitOfWork.Customers.GetAllWithAddressesAsync();


            var appUserIds = customers.Select(c => c.AppUserId).ToList();

            var users = await _userManager.Users
                .Where(u => appUserIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.NationalID,
                    u.Email,
                    u.UserName,
                    u.PhoneNumber
                })
                .ToListAsync();

            var userDict = users.ToDictionary(u => u.Id, u => u);

            var customerDtos = customers.Select(c => new CustomerDto
            {
                FullName = userDict.GetValueOrDefault(c.AppUserId)?.FullName ?? "N/A",
                NationalID = userDict.GetValueOrDefault(c.AppUserId)?.NationalID ?? "N/A",
                Email = userDict.GetValueOrDefault(c.AppUserId)?.Email ?? "N/A",
                UserName = userDict.GetValueOrDefault(c.AppUserId)?.UserName ?? "N/A",
                PhoneNumber = userDict.GetValueOrDefault(c.AppUserId)?.PhoneNumber ?? "N/A",
                Gender = c.Gender,
                Addresses = c.Addresses.Select(a => new GetAddressDto
                {
                    Id = a.Id,
                    Governorate = a.Governorate,
                    City = a.City,
                    District = a.District,
                    Street = a.Street,
                    BuildingNumber = a.BuildingNumber,
                    PostalCode = a.PostalCode
                }).ToList()
            });

            return customerDtos;


        }

    }
}
