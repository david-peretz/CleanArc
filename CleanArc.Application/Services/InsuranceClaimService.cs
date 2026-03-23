using CleanArc.Application.Abstractions;
using CleanArc.Application.Contracts;
using CleanArc.Domain.Claims;
using CleanArc.Domain.Enums;
using CleanArc.Domain.ValueObjects;

namespace CleanArc.Application.Services;

public sealed class InsuranceClaimService
{
    private readonly IInsuranceClaimRepository _repository;
    private readonly IPriorityScoringPolicy _priorityScoringPolicy;

    public InsuranceClaimService(
        IInsuranceClaimRepository repository,
        IPriorityScoringPolicy priorityScoringPolicy)
    {
        _repository = repository;
        _priorityScoringPolicy = priorityScoringPolicy;
    }

    public async Task<Guid> CreateAsync(
        CreateInsuranceClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = InsuranceClaim.Create(
            command.CitizenName,
            command.Description,
            command.Category,
            new Location(command.Street, command.CityArea),
            command.AffectsSensitivePopulation);

        var score = _priorityScoringPolicy.Calculate(request);
        request.SetPriority(score);

        await _repository.AddAsync(request, cancellationToken);
        return request.Id;
    }

    public async Task<InsuranceClaimDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken);
        return request is null ? null : Map(request);
    }

    public async Task<IReadOnlyList<InsuranceClaimDto>> GetOpenRequestsAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(cancellationToken);

        return all
            .Where(r => r.Status is RequestStatus.Opened or RequestStatus.InProgress)
            .OrderByDescending(r => r.PriorityScore)
            .ThenBy(r => r.CreatedAtUtc)
            .Select(Map)
            .ToList();
    }

    public async Task<bool> StartHandlingAsync(
        Guid id,
        StartHandlingCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        request.StartHandling(command.Department);
        await _repository.UpdateAsync(request, cancellationToken);
        return true;
    }

    public async Task<bool> ResolveAsync(
        Guid id,
        CloseInsuranceClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        request.Resolve(command.Notes);
        await _repository.UpdateAsync(request, cancellationToken);
        return true;
    }

    public async Task<bool> RejectAsync(
        Guid id,
        CloseInsuranceClaimCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = await _repository.GetByIdAsync(id, cancellationToken);
        if (request is null)
        {
            return false;
        }

        request.Reject(command.Notes);
        await _repository.UpdateAsync(request, cancellationToken);
        return true;
    }

    public async Task<InsuranceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var all = await _repository.GetAllAsync(cancellationToken);
        var closed = all.Where(x => x.ClosedAtUtc.HasValue).ToList();

        var avgHours = closed.Count == 0
            ? 0
            : closed.Average(x => (x.ClosedAtUtc!.Value - x.CreatedAtUtc).TotalHours);

        var hotspots = all
            .GroupBy(x => x.Location.CityArea)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new HotspotAreaDto(g.Key, g.Count()))
            .ToList();

        return new InsuranceDashboardDto(
            all.Count,
            all.Count(x => x.Status == RequestStatus.Opened),
            all.Count(x => x.Status == RequestStatus.InProgress),
            all.Count(x => x.Status == RequestStatus.Resolved),
            all.Count(x => x.Status == RequestStatus.Rejected),
            Math.Round(avgHours, 2),
            hotspots);
    }

    private static InsuranceClaimDto Map(InsuranceClaim request)
    {
        return new InsuranceClaimDto(
            request.Id,
            request.CitizenName,
            request.Description,
            request.Category,
            request.Location.Street,
            request.Location.CityArea,
            request.AffectsSensitivePopulation,
            request.PriorityScore,
            request.Status,
            request.AssignedDepartment,
            request.CreatedAtUtc,
            request.HandlingStartedAtUtc,
            request.ClosedAtUtc,
            request.ClosureNotes);
    }
}


