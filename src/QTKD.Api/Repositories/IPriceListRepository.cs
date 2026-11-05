using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public interface IPriceListRepository
{
    Task<List<PriceList>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriceList?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<PriceItem>> GetItemsByPriceListIdAsync(int priceListId, CancellationToken cancellationToken = default);
    Task<bool> ExistsItemByServiceCodeAsync(int priceListId, string serviceCode, CancellationToken cancellationToken = default);
    Task AddPriceListAsync(PriceList priceList, CancellationToken cancellationToken = default);
    Task AddPriceItemAsync(PriceItem priceItem, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
