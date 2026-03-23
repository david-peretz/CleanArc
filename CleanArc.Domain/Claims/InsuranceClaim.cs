using CleanArc.Domain.Common;
using CleanArc.Domain.Enums;
using CleanArc.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanArc.Domain.Claims;

public sealed class InsuranceClaim
{
    private InsuranceClaim()
    {
        CitizenName = string.Empty;
        Description = string.Empty;
        Location = new Location("Unknown", "Unknown");
        DecisionReason = string.Empty;
        DecisionHistory = new List<string>();
    }

    private InsuranceClaim(
        Guid id,
        int age,
        int claims,
        decimal amount,
        string citizenName,
        string description,
        RequestCategory category,
        Location location,
        bool affectsSensitivePopulation)
    {
        Id = id;
        CitizenName = citizenName;
        Description = description;
        Category = category;
        Location = location;
        AffectsSensitivePopulation = affectsSensitivePopulation;
        Age = age;
        Claims = claims;
        Amount = amount;
        DecisionHistory = new List<string>();
        Status = RequestStatus.Opened;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string CitizenName { get; private set; }
    public string Description { get; private set; }
    public RequestCategory Category { get; private set; }
    public Location Location { get; private set; }
    public bool AffectsSensitivePopulation { get; private set; }
    public int Age { get; private set; }
    public int Claims { get; private set; }
    public decimal Amount { get; private set; }
    public double? RiskScore { get; private set; }
    public RiskDecision? RiskDecision { get; private set; }
    public string DecisionReason { get; private set; } = string.Empty;
    [NotMapped]
    public IReadOnlyCollection<string> DecisionHistory { get; private set; }
    public RequestStatus Status { get; private set; }
    public string? AssignedDepartment { get; private set; }
    public int PriorityScore { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? HandlingStartedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public string? ClosureNotes { get; private set; }

    public static InsuranceClaim Create(
        string citizenName,
        string description,
        RequestCategory category,
        Location location,
        bool affectsSensitivePopulation)
    {
        if (string.IsNullOrWhiteSpace(citizenName))
        {
            throw new DomainRuleException("Citizen name is required.");
        }

        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length < 10)
        {
            throw new DomainRuleException("Description must include at least 10 characters.");
        }

        return new InsuranceClaim(
            Guid.NewGuid(),
            age: 0,
            claims: 0,
            amount: 0,
            citizenName.Trim(),
            description.Trim(),
            category,
            location,
            affectsSensitivePopulation);
    }

    public static InsuranceClaim CreateRiskAssessment(int age, int claims, decimal amount)
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

        return new InsuranceClaim(
            Guid.NewGuid(),
            age,
            claims,
            amount,
            citizenName: "AI Risk Input",
            description: "Auto-generated claim context for risk decisioning.",
            category: RequestCategory.Sanitation,
            location: new Location("N/A", "N/A"),
            affectsSensitivePopulation: false);
    }

    public void ApplyRiskOutcome(double score, RiskDecision decision, string reason)
    {
        if (score is < 0 or > 1)
        {
            throw new DomainRuleException("Risk score must be in range [0,1].");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Decision reason is required.");
        }

        RiskScore = score;
        RiskDecision = decision;
        DecisionReason = reason.Trim();

        var history = DecisionHistory.ToList();
        history.Insert(0, $"{DateTime.UtcNow:O} | {decision} | {score:F2} | {DecisionReason}");
        DecisionHistory = history.Take(20).ToList();
    }

    public void SetPriority(int score)
    {
        if (score < 0 || score > 100)
        {
            throw new DomainRuleException("Priority score must be between 0 and 100.");
        }

        PriorityScore = score;
    }

    public void StartHandling(string department)
    {
        if (Status is RequestStatus.Resolved or RequestStatus.Rejected)
        {
            throw new DomainRuleException("Closed request cannot move back to handling.");
        }

        if (string.IsNullOrWhiteSpace(department))
        {
            throw new DomainRuleException("Department is required.");
        }

        AssignedDepartment = department.Trim();
        Status = RequestStatus.InProgress;
        HandlingStartedAtUtc = DateTime.UtcNow;
    }

    public void Resolve(string closureNotes)
    {
        if (Status == RequestStatus.Resolved)
        {
            throw new DomainRuleException("Request is already resolved.");
        }

        if (Status == RequestStatus.Rejected)
        {
            throw new DomainRuleException("Rejected request cannot be resolved.");
        }

        if (string.IsNullOrWhiteSpace(closureNotes))
        {
            throw new DomainRuleException("Resolution notes are required.");
        }

        Status = RequestStatus.Resolved;
        ClosedAtUtc = DateTime.UtcNow;
        ClosureNotes = closureNotes.Trim();
    }

    public void Reject(string reason)
    {
        if (Status == RequestStatus.Resolved)
        {
            throw new DomainRuleException("Resolved request cannot be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainRuleException("Rejection reason is required.");
        }

        Status = RequestStatus.Rejected;
        ClosedAtUtc = DateTime.UtcNow;
        ClosureNotes = reason.Trim();
    }
}

