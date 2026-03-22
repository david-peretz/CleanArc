namespace CleanArc.Infrastructure.AiClient;

public sealed class PythonRiskServiceOptions
{
    public const string SectionName = "PythonRiskService";

    public string BaseUrl { get; init; } = "http://localhost:8000";
    public string AnalyzePath { get; init; } = "/api/risk/analyze";
    public int TimeoutSeconds { get; init; } = 10;
    public int MaxRetries { get; init; } = 3;
}
