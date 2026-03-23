using CleanArc.Application.Contracts;
using CleanArc.Application.Services;
using CleanArc.Domain.Enums;
using CleanArc.Infrastructure.Policies;
using CleanArc.Infrastructure.Repositories;

var repository = new InMemoryInsuranceClaimRepository();
var scoringPolicy = new InsuranceClaimPriorityScoringPolicy();
var service = new InsuranceClaimService(repository, scoringPolicy);

var id = await service.CreateAsync(
    new CreateInsuranceClaimCommand(
        "Dana Levi",
        "Broken traffic light on Ibn Gabirol near school crossing.",
        RequestCategory.StreetLighting,
        "Ibn Gabirol 118",
        "Center",
        true));

var started = await service.StartHandlingAsync(id, new StartHandlingCommand("Electricity Department"));
var resolved = await service.ResolveAsync(id, new CloseInsuranceClaimCommand("Technician replaced the damaged switch."));
var dashboard = await service.GetDashboardAsync();

if (!started || !resolved || dashboard.Resolved != 1)
{
    Console.Error.WriteLine("Smoke test failed.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Smoke test passed.");
Console.WriteLine($"Avg close time (hours): {dashboard.AvgHoursToClose}");


