using CleanArc.Application.Reporting.Handlers;
using CleanArc.Application.Reporting.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArc.WebApi.Controllers;

[ApiController]
[Route("api/reports/risk")]
public sealed class RiskReportsController : ControllerBase
{
    private readonly GetLargeRiskReportQueryHandler _handler;

    public RiskReportsController(GetLargeRiskReportQueryHandler handler)
    {
        _handler = handler;
    }

    [AllowAnonymous]
    [HttpGet("large")]
    [ProducesResponseType(typeof(LargeRiskReportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLarge(
        [FromQuery] int pageSize = 200,
        [FromQuery] DateTime? updatedAfterUtc = null,
        [FromQuery] Guid? cursorAfterId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _handler.HandleAsync(
            new GetLargeRiskReportQuery(pageSize, updatedAfterUtc, cursorAfterId),
            cancellationToken);

        return Ok(result);
    }
}
