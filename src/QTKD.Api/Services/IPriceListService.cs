using QTKD.Api.DTOs.PriceLists;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<List<PriceListResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriceListResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PriceListResponse> CreateAsync(CreatePriceListRequest request, CancellationToken cancellationToken = default);
    Task<List<PriceItemResponse>> GetItemsAsync(int priceListId, CancellationToken cancellationToken = default);
    Task<PriceItemResponse> AddItemAsync(int priceListId, CreatePriceItemRequest request, CancellationToken cancellationToken = default);
}
