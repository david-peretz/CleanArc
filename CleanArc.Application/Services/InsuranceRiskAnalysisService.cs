using CleanArc.Application.Contracts;
using CleanArc.Application.Interfaces;
using CleanArc.Domain.Common;
using CleanArc.Domain.Enums;

namespace CleanArc.Application.Services;

public sealed class InsuranceRiskAnalysisService
{
    private readonly IPythonRiskAgentClient _pythonRiskAgentClient;

    public InsuranceRiskAnalysisService(IPythonRiskAgentClient pythonRiskAgentClient)
    {
        _pythonRiskAgentClient = pythonRiskAgentClient;
    }

    public async Task<RiskAssessmentResultDto> AnalyzeAsync(
        RiskAnalysisCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateInput(command);

        var aiResult = await _pythonRiskAgentClient.AnalyzeAsync(command, cancellationToken);
        var decision = ValidateOutput(aiResult);
        return new RiskAssessmentResultDto(aiResult.Score, decision, aiResult.Reason.Trim());
    }

    private static void ValidateInput(RiskAnalysisCommand command)
    {
        if (command.Age < 18 || command.Age > 120)
        {
            throw new DomainRuleException("Age must be between 18 and 120.");
        }

        if (command.Claims < 0 || command.Claims > 100)
        {
            throw new DomainRuleException("Claims must be between 0 and 100.");
        }

        if (command.Amount < 0)
        {
            throw new DomainRuleException("Amount must be zero or above.");
        }
    }

    private static RiskDecision ValidateOutput(PythonRiskResult aiResult)
    {
        if (aiResult.Score is < 0 or > 1)
        {
            throw new DomainRuleException("AI score must be in range [0,1].");
        }

        if (string.IsNullOrWhiteSpace(aiResult.Decision))
        {
            throw new DomainRuleException("AI decision is required.");
        }

        if (!Enum.TryParse<RiskDecision>(aiResult.Decision.Trim(), ignoreCase: true, out var decision))
        {
            throw new DomainRuleException("AI decision must be one of: Approve, Review, Reject.");
        }

        if (string.IsNullOrWhiteSpace(aiResult.Reason))
        {
            throw new DomainRuleException("AI reason is required.");
        }

        return decision;
    }
}
