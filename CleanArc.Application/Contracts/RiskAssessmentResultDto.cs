using CleanArc.Domain.Enums;

namespace CleanArc.Application.Contracts;

public sealed record RiskAssessmentResultDto(
    double Score,
    RiskDecision Decision,
    string Reason);
