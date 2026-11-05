namespace CleanArc.Application.Orchestration.Commands;

public sealed record EnqueueRiskAnalysisSagaCommand(int Age, int Claims, decimal Amount);
