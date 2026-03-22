using CleanArc.Domain.Common;
using CleanArc.Domain.Enums;
using CleanArc.Domain.ValueObjects;

namespace CleanArc.Domain.Entities;

public sealed class ServiceRequest
{
    private ServiceRequest()
    {
        CitizenName = string.Empty;
        Description = string.Empty;
        Location = new Location("Unknown", "Unknown");
    }

    private ServiceRequest(
        Guid id,
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
        Status = RequestStatus.Opened;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string CitizenName { get; private set; }
    public string Description { get; private set; }
    public RequestCategory Category { get; private set; }
    public Location Location { get; private set; }
    public bool AffectsSensitivePopulation { get; private set; }
    public RequestStatus Status { get; private set; }
    public string? AssignedDepartment { get; private set; }
    public int PriorityScore { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? HandlingStartedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public string? ClosureNotes { get; private set; }

    public static ServiceRequest Create(
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

        return new ServiceRequest(
            Guid.NewGuid(),
            citizenName.Trim(),
            description.Trim(),
            category,
            location,
            affectsSensitivePopulation);
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
