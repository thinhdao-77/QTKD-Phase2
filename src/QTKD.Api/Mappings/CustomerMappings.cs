using QTKD.Api.DTOs.Customers;
using QTKD.Api.Entities;

namespace QTKD.Api.Mappings;

public static class CustomerMappings
{
    public static CustomerResponse ToResponse(this Customer customer) =>
        new(
            customer.Id,
            customer.CustomerCode,
            customer.Name,
            customer.TaxCode,
            customer.Address,
            customer.ContactEmail,
            customer.Status,
            customer.CreatedAt
        );
}

