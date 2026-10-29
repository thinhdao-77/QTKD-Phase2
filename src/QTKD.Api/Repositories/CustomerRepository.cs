using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Entities;

namespace QTKD.Api.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly QtkdDbContext _context;

    public CustomerRepository(QtkdDbContext context)
    {
        _context = context;
    }

    public Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}

