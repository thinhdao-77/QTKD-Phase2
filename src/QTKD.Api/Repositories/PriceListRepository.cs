using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public sealed class PriceListRepository : IPriceListRepository
{
    private readonly QtkdDbContext _context;

    public PriceListRepository(QtkdDbContext context)
    {
        _context = context;
    }

    public Task<List<PriceList>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.PriceLists
            .AsNoTracking()
            .Include(p => p.Items)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public Task<PriceList?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.PriceLists
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<PriceItem>> GetItemsByPriceListIdAsync(int priceListId, CancellationToken cancellationToken = default) =>
        _context.PriceItems
            .AsNoTracking()
            .Where(pi => pi.PriceListId == priceListId)
            .OrderBy(pi => pi.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsItemByServiceCodeAsync(int priceListId, string serviceCode, CancellationToken cancellationToken = default) =>
        _context.PriceItems
            .AnyAsync(pi => pi.PriceListId == priceListId && pi.ServiceCode.ToLower() == serviceCode.ToLower(), cancellationToken);

    public async Task AddPriceListAsync(PriceList priceList, CancellationToken cancellationToken = default) =>
        await _context.PriceLists.AddAsync(priceList, cancellationToken);

    public async Task AddPriceItemAsync(PriceItem priceItem, CancellationToken cancellationToken = default) =>
        await _context.PriceItems.AddAsync(priceItem, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
