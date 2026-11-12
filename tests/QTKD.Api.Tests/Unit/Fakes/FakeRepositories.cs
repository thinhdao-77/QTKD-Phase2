using QTKD.Api.Entities;
using QTKD.Api.Repositories;

namespace QTKD.Api.Tests.Unit.Fakes;

/// <summary>
/// Fake in-memory repository cho Customer để chạy Unit Test tầng Service độc lập với PostgreSQL.
/// </summary>
public sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();
    private int _nextId = 1;

    public FakeCustomerRepository(IEnumerable<Customer>? seed = null)
    {
        if (seed is not null)
        {
            foreach (var c in seed)
            {
                if (c.Id == 0) c.Id = _nextId++;
                else if (c.Id >= _nextId) _nextId = c.Id + 1;
                _customers.Add(c);
            }
        }
    }

    public Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_customers.OrderBy(c => c.Id).ToList());

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsByCodeAsync(string customerCode, CancellationToken cancellationToken = default) =>
        Task.FromResult(_customers.Any(c => string.Equals(c.CustomerCode, customerCode, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (customer.Id == 0)
        {
            customer.Id = _nextId++;
        }
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

/// <summary>
/// Fake in-memory repository cho Contract để chạy Unit Test các quy tắc nghiệp vụ CTR-01, CTR-02, CTR-03.
/// </summary>
public sealed class FakeContractRepository : IContractRepository
{
    private readonly List<Contract> _contracts = new();
    private int _nextId = 1;

    public FakeContractRepository(IEnumerable<Contract>? seed = null)
    {
        if (seed is not null)
        {
            foreach (var c in seed)
            {
                if (c.Id == 0) c.Id = _nextId++;
                else if (c.Id >= _nextId) _nextId = c.Id + 1;
                _contracts.Add(c);
            }
        }
    }

    public Task<List<Contract>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_contracts.OrderBy(c => c.Id).ToList());

    public Task<Contract?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contracts.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsByContractNoAsync(string contractNo, CancellationToken cancellationToken = default) =>
        Task.FromResult(_contracts.Any(c => string.Equals(c.ContractNo, contractNo, StringComparison.OrdinalIgnoreCase)));

    public Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        if (contract.Id == 0)
        {
            contract.Id = _nextId++;
        }
        _contracts.Add(contract);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

