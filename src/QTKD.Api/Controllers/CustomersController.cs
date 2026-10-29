using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.Customers;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CustomerResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        return Ok(customers);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(customer);
    }
}

