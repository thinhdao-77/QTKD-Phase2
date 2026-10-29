using QTKD.Api.DTOs.Contracts;
using QTKD.Api.Entities;

namespace QTKD.Api.Mappings;

public static class ContractMappings
{
    public static ContractResponse ToResponse(this Contract contract) =>
        new(
            contract.Id,
            contract.ContractNo,
            contract.CustomerId,
            contract.Customer?.Name ?? string.Empty,
            contract.Customer?.Status ?? string.Empty,
            contract.StartDate,
            contract.EndDate,
            contract.ServiceGroup,
            contract.ContentSummary,
            contract.AttachmentUrl,
            contract.Status,
            contract.SubmittedAt,
            contract.CreatedAt
        );
}

