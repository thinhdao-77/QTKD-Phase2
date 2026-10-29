namespace QTKD.Api.DTOs.Contracts;

public sealed record ContractResponse(
    int Id,
    string ContractNo,
    int CustomerId,
    string CustomerName,
    string CustomerStatus,
    DateOnly StartDate,
    DateOnly EndDate,
    string? ServiceGroup,
    string? ContentSummary,
    string? AttachmentUrl,
    string Status,
    DateTime? SubmittedAt,
    DateTime CreatedAt
);

