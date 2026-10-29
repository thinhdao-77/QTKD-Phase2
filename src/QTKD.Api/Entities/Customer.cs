namespace QTKD.Api.Entities;

public class Customer
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string Status { get; set; } = CustomerStatus.Active;
    public DateTime CreatedAt { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}

public static class CustomerStatus
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Inactive = "Inactive";
}

