using QTKD.Api.DTOs.PriceLists;
using QTKD.Api.Entities;

namespace QTKD.Api.Mappings;

public static class PriceListMappings
{
    public static PriceItemResponse ToResponse(this PriceItem item) =>
        new(
            item.Id,
            item.PriceListId,
            item.ServiceCode,
            item.ServiceName,
            item.Unit,
            item.UnitPrice
        );

    public static PriceListResponse ToResponse(this PriceList priceList) =>
        new(
            priceList.Id,
            priceList.Name,
            priceList.EffectiveDate,
            priceList.ExpiryDate,
            priceList.Status,
            priceList.CreatedAt,
            priceList.Items.OrderBy(i => i.Id).Select(i => i.ToResponse()).ToList()
        );
}

