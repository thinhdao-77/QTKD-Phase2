using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string customerCode, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
