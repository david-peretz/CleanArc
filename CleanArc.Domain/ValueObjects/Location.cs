using CleanArc.Domain.Common;

namespace CleanArc.Domain.ValueObjects;

public sealed record Location
{
    private Location()
    {
        Street = string.Empty;
        CityArea = string.Empty;
    }

    public string Street { get; private set; }
    public string CityArea { get; private set; }

    public Location(string street, string cityArea)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new DomainRuleException("Street is required.");
        }

        if (string.IsNullOrWhiteSpace(cityArea))
        {
            throw new DomainRuleException("City area is required.");
        }

        Street = street.Trim();
        CityArea = cityArea.Trim();
    }
}
