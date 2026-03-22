using CleanArc.Application.Abstractions;
using CleanArc.Domain.Entities;
using CleanArc.Domain.Enums;

namespace CleanArc.Infrastructure.Policies;

public sealed class MunicipalPriorityScoringPolicy : IPriorityScoringPolicy
{
    public int Calculate(ServiceRequest request)
    {
        var baseScore = request.Category switch
        {
            RequestCategory.RoadHazard => 80,
            RequestCategory.StreetLighting => 65,
            RequestCategory.Sanitation => 55,
            RequestCategory.Noise => 45,
            RequestCategory.Graffiti => 35,
            _ => 30
        };

        if (request.AffectsSensitivePopulation)
        {
            baseScore += 15;
        }

        // Crowded areas in Tel Aviv usually need faster SLA.
        if (request.Location.CityArea.Contains("????", StringComparison.OrdinalIgnoreCase)
            || request.Location.CityArea.Contains("Center", StringComparison.OrdinalIgnoreCase))
        {
            baseScore += 5;
        }

        return Math.Min(baseScore, 100);
    }
}
