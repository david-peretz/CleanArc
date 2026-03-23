using CleanArc.Domain.Claims;

namespace CleanArc.Application.Abstractions;

public interface IPriorityScoringPolicy
{
    int Calculate(InsuranceClaim request);
}

