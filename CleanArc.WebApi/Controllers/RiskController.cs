using CleanArc.Application.Contracts;
using CleanArc.Application.Orchestration.Commands;
using CleanArc.Application.Orchestration.Handlers;
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
    private readonly EnqueueRiskAnalysisSagaHandler _enqueueHandler;
    private readonly GetRiskAnalysisSagaHandler _getHandler;

    public RiskController(
        InsuranceRiskAnalysisService service,
        EnqueueRiskAnalysisSagaHandler enqueueHandler,
        GetRiskAnalysisSagaHandler getHandler)
    {
        _service = service;
        _enqueueHandler = enqueueHandler;
        _getHandler = getHandler;
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

    [AllowAnonymous]
    [HttpPost("async")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Enqueue(
        [FromBody] AnalyzeRiskRequest request,
        CancellationToken cancellationToken)
    {
        var sagaId = await _enqueueHandler.HandleAsync(
            new EnqueueRiskAnalysisSagaCommand(request.Age, request.Claims, request.Amount),
            cancellationToken);

        return Accepted(new { jobId = sagaId, status = "Queued" });
    }

    [AllowAnonymous]
    [HttpGet("jobs/{jobId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken cancellationToken)
    {
        var saga = await _getHandler.HandleAsync(jobId, cancellationToken);
        if (saga is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            jobId = saga.Id,
            status = saga.Status.ToString(),
            score = saga.Score,
            decision = saga.Decision?.ToString(),
            reason = saga.Reason,
            error = saga.Error,
            createdAtUtc = saga.CreatedAtUtc,
            updatedAtUtc = saga.UpdatedAtUtc
        });
    }
}
