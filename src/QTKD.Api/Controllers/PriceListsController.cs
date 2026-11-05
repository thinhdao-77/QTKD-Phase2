using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.PriceLists;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/pricelists")]
public sealed class PriceListsController : ControllerBase
{
    private readonly IPriceListService _priceListService;

    public PriceListsController(IPriceListService priceListService)
    {
        _priceListService = priceListService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PriceListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PriceListResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var priceLists = await _priceListService.GetAllAsync(cancellationToken);
        return Ok(priceLists);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PriceListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceListResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var priceList = await _priceListService.GetByIdAsync(id, cancellationToken);
        return Ok(priceList);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PriceListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceListResponse>> Create(
        [FromBody] CreatePriceListRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _priceListService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:int}/items")]
    [ProducesResponseType(typeof(List<PriceItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PriceItemResponse>>> GetItems(int id, CancellationToken cancellationToken)
    {
        var items = await _priceListService.GetItemsAsync(id, cancellationToken);
        return Ok(items);
    }

    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(PriceItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PriceItemResponse>> AddItem(
        int id,
        [FromBody] CreatePriceItemRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _priceListService.AddItemAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetItems), new { id }, created);
    }
}
