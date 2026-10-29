namespace QTKD.Api.Entities;

public class Contract
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? ServiceGroup { get; set; }
    public string? ContentSummary { get; set; }
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = ContractStatus.Draft;
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Customer Customer { get; set; } = null!;
}

public static class ContractStatus
{
    public const string Draft = "Draft";
    public const string PendingApproval = "PendingApproval";
    public const string RevisionRequested = "RevisionRequested";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}

