using QTKD.Api.DTOs.Contracts;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<List<ContractResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ContractResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

