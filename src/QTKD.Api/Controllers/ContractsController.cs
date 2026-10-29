using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.Contracts;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public sealed class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ContractResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContractResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var contracts = await _contractService.GetAllAsync(cancellationToken);
        return Ok(contracts);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetByIdAsync(id, cancellationToken);
        return Ok(contract);
    }
}

