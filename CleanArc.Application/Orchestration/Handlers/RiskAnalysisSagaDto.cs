using CleanArc.Application.Orchestration.Commands;
using CleanArc.Domain.Enums;

namespace CleanArc.Application.Orchestration.Handlers;

public sealed record RiskAnalysisSagaDto(
    Guid Id,
    int Age,
    int Claims,
    decimal Amount,
    RiskSagaStatus Status,
    double? Score,
    RiskDecision? Decision,
    string Reason,
    string Error,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
