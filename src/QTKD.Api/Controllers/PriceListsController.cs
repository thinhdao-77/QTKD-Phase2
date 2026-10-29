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
}

