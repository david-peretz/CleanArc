namespace CleanArc.Application.Contracts;

public sealed record PythonRiskResult(
    double Score,
    string Reason);
