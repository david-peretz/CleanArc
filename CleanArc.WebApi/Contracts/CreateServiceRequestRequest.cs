namespace CleanArc.WebApi.Contracts;

public sealed record CreateServiceRequestRequest(
    string CitizenName,
    string Description,
    int Category,
    string Street,
    string CityArea,
    bool AffectsSensitivePopulation);
