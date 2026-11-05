using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public sealed class ContractRepository : IContractRepository
{
    private readonly QtkdDbContext _context;

    public ContractRepository(QtkdDbContext context)
    {
        _context = context;
    }

    public Task<List<Contract>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Contracts
            .AsNoTracking()
            .Include(c => c.Customer)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Contracts
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsByContractNoAsync(string contractNo, CancellationToken cancellationToken = default) =>
        _context.Contracts
            .AnyAsync(c => c.ContractNo.ToLower() == contractNo.ToLower(), cancellationToken);

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default) =>
        await _context.Contracts.AddAsync(contract, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
