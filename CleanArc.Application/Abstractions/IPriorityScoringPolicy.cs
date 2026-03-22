using CleanArc.Domain.Entities;

namespace CleanArc.Application.Abstractions;

public interface IPriorityScoringPolicy
{
    int Calculate(ServiceRequest request);
}
