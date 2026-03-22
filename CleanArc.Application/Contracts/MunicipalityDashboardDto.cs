namespace CleanArc.Application.Contracts;

public sealed record MunicipalityDashboardDto(
    int TotalRequests,
    int Opened,
    int InProgress,
    int Resolved,
    int Rejected,
    double AvgHoursToClose,
    IReadOnlyList<HotspotAreaDto> HotspotAreas);
