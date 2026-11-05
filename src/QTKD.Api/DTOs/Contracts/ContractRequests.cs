using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs.Contracts;

public sealed record CreateContractRequest(
    [Required(ErrorMessage = "ContractNo is required.")]
    [MaxLength(50)]
    string ContractNo,

    [Required]
    int CustomerId,

    [Required]
    DateOnly StartDate,

    [Required]
    DateOnly EndDate,

    [MaxLength(100)]
    string? ServiceGroup,

    [MaxLength(1000)]
    string? ContentSummary,

    [MaxLength(500)]
    string? AttachmentUrl,

    [MaxLength(50)]
    string? Status
);

public sealed record UpdateContractRequest(
    [Required]
    int CustomerId,

    [Required]
    DateOnly StartDate,

    [Required]
    DateOnly EndDate,

    [MaxLength(100)]
    string? ServiceGroup,

    [MaxLength(1000)]
    string? ContentSummary,

    [MaxLength(500)]
    string? AttachmentUrl,

    [MaxLength(50)]
    string? Status
);

public sealed record SubmitContractRequest(
    [MaxLength(500)]
    string? Comment
);

public sealed record SubmitContractResponse(
    int ContractId,
    string ContractNo,
    string PreviousStatus,
    string Status,
    string ApprovalId,
    DateTime SubmittedAt,
    string? Comment,
    string Message
);
