using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs.Customers;

public sealed record CreateCustomerRequest(
    [Required(ErrorMessage = "CustomerCode is required.")]
    [MaxLength(50)]
    string CustomerCode,

    [Required(ErrorMessage = "Customer name is required.")]
    [MaxLength(200)]
    string Name,

    [MaxLength(50)]
    string? TaxCode,

    [MaxLength(300)]
    string? Address,

    [EmailAddress(ErrorMessage = "ContactEmail is not a valid email address.")]
    [MaxLength(150)]
    string? ContactEmail,

    [MaxLength(50)]
    string? Status
);

public sealed record UpdateCustomerRequest(
    [Required(ErrorMessage = "Customer name is required.")]
    [MaxLength(200)]
    string Name,

    [MaxLength(50)]
    string? TaxCode,

    [MaxLength(300)]
    string? Address,

    [EmailAddress(ErrorMessage = "ContactEmail is not a valid email address.")]
    [MaxLength(150)]
    string? ContactEmail,

    [Required(ErrorMessage = "Customer status is required.")]
    [MaxLength(50)]
    string Status
);
