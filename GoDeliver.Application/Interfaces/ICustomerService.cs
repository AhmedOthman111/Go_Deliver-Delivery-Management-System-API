using GoDeliver.Application.Models.Customerservice;
using GoDeliver.Application.Models.shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Interfaces
{
    public interface ICustomerService
    {
     Task<CustomerDto> GetProfileAsync(Guid appUserId);
     Task AddAddressAsync(Guid appUserId, AddressDto dto);
     Task<IEnumerable<GetAddressDto>> GetAddressesAsync(Guid appUserId);
     Task UpdateCustomerinfoAsync(Guid appUserId,UpateCustomerInfoDto dto);
     Task<AddressDto> UpdateAddressAsync(Guid appUserId, Guid addressId, AddressDto dto);
     Task DeleteAddressAsync(Guid appUserId, Guid addressId);
     Task<IEnumerable<CustomerDto>> GetAllCustomers();


    }
}
