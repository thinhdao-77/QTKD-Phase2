namespace QTKD.Api.Entities;

public class PriceList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string Status { get; set; } = PriceListStatus.Draft;
    public DateTime CreatedAt { get; set; }

    public ICollection<PriceItem> Items { get; set; } = new List<PriceItem>();
}

public static class PriceListStatus
{
    public const string Draft = "Draft";
    public const string Effective = "Effective";
    public const string Expired = "Expired";
}

