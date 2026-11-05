using CleanArc.Domain.Common;
using CleanArc.Domain.Enums;

namespace CleanArc.Domain.Orchestration;

public sealed class RiskAnalysisSaga
{
    private RiskAnalysisSaga()
    {
        Error = string.Empty;
        Reason = string.Empty;
    }

    private RiskAnalysisSaga(Guid id, int age, int claims, decimal amount)
    {
        Id = id;
        Age = age;
        Claims = claims;
        Amount = amount;
        Status = RiskSagaStatus.Queued;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        Error = string.Empty;
        Reason = string.Empty;
    }

    public Guid Id { get; private set; }
    public int Age { get; private set; }
    public int Claims { get; private set; }
    public decimal Amount { get; private set; }
    public RiskSagaStatus Status { get; private set; }
    public double? Score { get; private set; }
    public RiskDecision? Decision { get; private set; }
    public string Reason { get; private set; }
    public string Error { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static RiskAnalysisSaga Enqueue(int age, int claims, decimal amount)
    {
        if (age is < 18 or > 120)
        {
            throw new DomainRuleException("Age must be between 18 and 120.");
        }

        if (claims is < 0 or > 100)
        {
            throw new DomainRuleException("Claims must be between 0 and 100.");
        }

        if (amount < 0)
        {
            throw new DomainRuleException("Amount must be zero or above.");
        }

        return new RiskAnalysisSaga(Guid.NewGuid(), age, claims, amount);
    }

    public void MarkRunning()
    {
        if (Status != RiskSagaStatus.Queued)
        {
            throw new DomainRuleException("Only queued saga can transition to running.");
        }

        Status = RiskSagaStatus.Running;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted(double score, RiskDecision decision, string reason)
    {
        if (Status != RiskSagaStatus.Running)
        {
            throw new DomainRuleException("Only running saga can complete.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Reason is required.");
        }

        Score = score;
        Decision = decision;
        Reason = reason.Trim();
        Error = string.Empty;
        Status = RiskSagaStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        if (Status != RiskSagaStatus.Running)
        {
            throw new DomainRuleException("Only running saga can fail.");
        }

        Error = string.IsNullOrWhiteSpace(error) ? "Unknown failure." : error.Trim();
        Status = RiskSagaStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
