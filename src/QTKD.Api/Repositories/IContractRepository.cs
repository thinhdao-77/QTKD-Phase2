using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
    Task<List<Contract>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByContractNoAsync(string contractNo, CancellationToken cancellationToken = default);
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
