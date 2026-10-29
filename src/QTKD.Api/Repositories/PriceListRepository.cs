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
}

