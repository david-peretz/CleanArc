using CleanArc.Application.Contracts;

namespace CleanArc.Application.Interfaces;

public interface IPythonRiskAgentClient
{
    Task<PythonRiskResult> AnalyzeAsync(
        RiskAnalysisCommand command,
        CancellationToken cancellationToken = default);
}
