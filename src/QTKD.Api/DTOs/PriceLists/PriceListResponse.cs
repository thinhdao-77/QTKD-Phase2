namespace QTKD.Api.DTOs.PriceLists;

public sealed record PriceItemResponse(
    int Id,
    int PriceListId,
    string ServiceCode,
    string ServiceName,
    string Unit,
    decimal UnitPrice
);

public sealed record PriceListResponse(
    int Id,
    string Name,
    DateOnly EffectiveDate,
    DateOnly? ExpiryDate,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<PriceItemResponse> Items
);

