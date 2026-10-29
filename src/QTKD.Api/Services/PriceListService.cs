using QTKD.Api.DTOs.PriceLists;
using QTKD.Api.Exceptions;
using QTKD.Api.Mappings;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class PriceListService : IPriceListService
{
    private readonly IPriceListRepository _priceListRepository;

    public PriceListService(IPriceListRepository priceListRepository)
    {
        _priceListRepository = priceListRepository;
    }

    public async Task<List<PriceListResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var priceLists = await _priceListRepository.GetAllAsync(cancellationToken);
        return priceLists.Select(p => p.ToResponse()).ToList();
    }

    public async Task<PriceListResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var priceList = await _priceListRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PriceList", id);

        return priceList.ToResponse();
    }
}

