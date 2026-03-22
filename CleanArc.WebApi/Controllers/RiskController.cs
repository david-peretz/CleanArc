using CleanArc.Application.Contracts;
using CleanArc.Application.Services;
using CleanArc.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArc.WebApi.Controllers;

[ApiController]
[Route("api/risk")]
public sealed class RiskController : ControllerBase
{
    private readonly InsuranceRiskAnalysisService _service;

    public RiskController(InsuranceRiskAnalysisService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(RiskAssessmentResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalyzeRiskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.AnalyzeAsync(
            new RiskAnalysisCommand(request.Age, request.Claims, request.Amount),
            cancellationToken);

        return Ok(new
        {
            score = result.Score,
            decision = result.Decision.ToString(),
            reason = result.Reason
        });
    }
}
