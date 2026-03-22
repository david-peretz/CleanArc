namespace CleanArc.Application.Contracts;

public sealed record RiskAnalysisCommand(
    int Age,
    int Claims,
    decimal Amount);
