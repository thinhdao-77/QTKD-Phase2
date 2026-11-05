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

    [HttpPost]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ContractResponse>> Create(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _contractService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ContractResponse>> Update(
        int id,
        [FromBody] UpdateContractRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _contractService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// US03 – Gửi hợp đồng trình duyệt (Draft / RevisionRequested -> PendingApproval).
    /// Kiểm tra CTR-02, CTR-03 và chống gửi trùng lặp (AC05).
    /// </summary>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(SubmitContractResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SubmitContractResponse>> Submit(
        int id,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] SubmitContractRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _contractService.SubmitAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
