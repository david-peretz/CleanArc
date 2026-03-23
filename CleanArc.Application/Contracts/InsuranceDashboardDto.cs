namespace CleanArc.Application.Contracts;

public sealed record InsuranceDashboardDto(
    int TotalRequests,
    int Opened,
    int InProgress,
    int Resolved,
    int Rejected,
    double AvgHoursToClose,
    IReadOnlyList<HotspotAreaDto> HotspotAreas);

