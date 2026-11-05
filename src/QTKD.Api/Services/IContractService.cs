using QTKD.Api.DTOs.Contracts;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<List<ContractResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ContractResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ContractResponse> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default);
    Task<ContractResponse> UpdateAsync(int id, UpdateContractRequest request, CancellationToken cancellationToken = default);
    Task<SubmitContractResponse> SubmitAsync(int id, SubmitContractRequest? request = null, CancellationToken cancellationToken = default);
}
