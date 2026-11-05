using QTKD.Api.DTOs.Customers;

namespace QTKD.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CustomerResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
    Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);
}
