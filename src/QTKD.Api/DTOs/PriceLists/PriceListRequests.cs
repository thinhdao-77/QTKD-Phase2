using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs.PriceLists;

public sealed record CreatePriceListRequest(
    [Required(ErrorMessage = "PriceList name is required.")]
    [MaxLength(200)]
    string Name,

    [Required]
    DateOnly EffectiveDate,

    DateOnly? ExpiryDate,

    [MaxLength(50)]
    string? Status
);

public sealed record CreatePriceItemRequest(
    [Required(ErrorMessage = "ServiceCode is required.")]
    [MaxLength(50)]
    string ServiceCode,

    [Required(ErrorMessage = "ServiceName is required.")]
    [MaxLength(200)]
    string ServiceName,

    [Required(ErrorMessage = "Unit is required.")]
    [MaxLength(50)]
    string Unit,

    [Required]
    decimal UnitPrice
);
