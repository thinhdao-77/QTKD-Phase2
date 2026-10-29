using QTKD.Api.DTOs.Contracts;
using QTKD.Api.Exceptions;
using QTKD.Api.Mappings;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;

    public ContractService(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    public async Task<List<ContractResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepository.GetAllAsync(cancellationToken);
        return contracts.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ContractResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract", id);

        return contract.ToResponse();
    }
}

