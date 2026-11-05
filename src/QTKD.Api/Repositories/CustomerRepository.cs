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

    public Task<bool> ExistsByCodeAsync(string customerCode, CancellationToken cancellationToken = default) =>
        _context.Customers
            .AnyAsync(c => c.CustomerCode.ToLower() == customerCode.ToLower(), cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await _context.Customers.AddAsync(customer, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
