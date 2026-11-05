namespace CleanArc.Domain.Reporting;

public sealed class RiskReportProjection
{
    public Guid Id { get; set; }
    public Guid SagaId { get; set; }
    public int Age { get; set; }
    public int Claims { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? Score { get; set; }
    public string? Decision { get; set; }
    public string? Reason { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
