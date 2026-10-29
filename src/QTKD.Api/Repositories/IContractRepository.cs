using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
    Task<List<Contract>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

