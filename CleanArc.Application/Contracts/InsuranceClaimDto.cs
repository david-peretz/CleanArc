using CleanArc.Domain.Enums;

namespace CleanArc.Application.Contracts;

public sealed record InsuranceClaimDto(
    Guid Id,
    string CitizenName,
    string Description,
    RequestCategory Category,
    string Street,
    string CityArea,
    bool AffectsSensitivePopulation,
    int PriorityScore,
    RequestStatus Status,
    string? AssignedDepartment,
    DateTime CreatedAtUtc,
    DateTime? HandlingStartedAtUtc,
    DateTime? ClosedAtUtc,
    string? ClosureNotes);

