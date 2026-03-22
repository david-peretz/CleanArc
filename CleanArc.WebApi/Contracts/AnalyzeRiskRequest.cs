using System.ComponentModel.DataAnnotations;

namespace CleanArc.WebApi.Contracts;

public sealed record AnalyzeRiskRequest(
    [property: Range(18, 120)] int Age,
    [property: Range(0, 100)] int Claims,
    [property: Range(0, double.MaxValue)] decimal Amount);
