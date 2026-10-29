using QTKD.Api.DTOs.Customers;
using QTKD.Api.Exceptions;
using QTKD.Api.Mappings;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(c => c.ToResponse()).ToList();
    }

    public async Task<CustomerResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        return customer.ToResponse();
    }
}

