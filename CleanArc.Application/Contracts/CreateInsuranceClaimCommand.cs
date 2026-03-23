using CleanArc.Domain.Enums;

namespace CleanArc.Application.Contracts;

public sealed record CreateInsuranceClaimCommand(
    string CitizenName,
    string Description,
    RequestCategory Category,
    string Street,
    string CityArea,
    bool AffectsSensitivePopulation);

