using CleanArc.Application.Contracts;
using CleanArc.Application.Services;
using CleanArc.Domain.Enums;
using CleanArc.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArc.WebApi.Controllers;

[ApiController]
[Route("api/service-requests")]
public sealed class InsuranceClaimsController : ControllerBase
{
    private readonly InsuranceClaimService _service;

    public InsuranceClaimsController(InsuranceClaimService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInsuranceClaimRequest request,
        CancellationToken cancellationToken)
    {
        var category = (RequestCategory)request.Category;

        var id = await _service.CreateAsync(
            new CreateInsuranceClaimCommand(
                request.CitizenName,
                request.Description,
                category,
                request.Street,
                request.CityArea,
                request.AffectsSensitivePopulation),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [Authorize(Roles = "Dispatcher,Manager")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InsuranceClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Dispatcher,Manager")]
    [HttpGet("open")]
    [ProducesResponseType(typeof(IReadOnlyList<InsuranceClaimDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpen(CancellationToken cancellationToken)
    {
        var result = await _service.GetOpenRequestsAsync(cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Dispatcher,Manager")]
    [HttpPatch("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartHandling(
        Guid id,
        [FromBody] StartHandlingRequest request,
        CancellationToken cancellationToken)
    {
        var ok = await _service.StartHandlingAsync(
            id,
            new StartHandlingCommand(request.Department),
            cancellationToken);

        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Manager")]
    [HttpPatch("{id:guid}/resolve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Resolve(
        Guid id,
        [FromBody] CloseInsuranceClaimRequest request,
        CancellationToken cancellationToken)
    {
        var ok = await _service.ResolveAsync(
            id,
            new CloseInsuranceClaimCommand(request.Notes),
            cancellationToken);

        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Manager")]
    [HttpPatch("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] CloseInsuranceClaimRequest request,
        CancellationToken cancellationToken)
    {
        var ok = await _service.RejectAsync(
            id,
            new CloseInsuranceClaimCommand(request.Notes),
            cancellationToken);

        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(InsuranceDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _service.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }
}


