using QTKD.Api.DTOs.PriceLists;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<List<PriceListResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriceListResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

