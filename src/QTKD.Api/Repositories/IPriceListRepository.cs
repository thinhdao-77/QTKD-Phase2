using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IPriceListRepository
{
    Task<List<PriceList>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

