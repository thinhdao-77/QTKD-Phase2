namespace QTKD.Api.DTOs.Customers;

public sealed record CustomerResponse(
    int Id,
    string CustomerCode,
    string Name,
    string? TaxCode,
    string? Address,
    string? ContactEmail,
    string Status,
    DateTime CreatedAt
);

