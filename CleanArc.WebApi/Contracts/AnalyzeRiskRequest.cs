using System.ComponentModel.DataAnnotations;

namespace CleanArc.WebApi.Contracts;

public sealed record AnalyzeRiskRequest(
    [Range(18, 120)] int Age,
    [Range(0, 100)] int Claims,
    [Range(0, double.MaxValue)] decimal Amount);
