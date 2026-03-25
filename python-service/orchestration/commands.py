from dataclasses import dataclass

from models.risk_models import RiskRequest, RiskResponse


@dataclass(frozen=True)
class AnalyzeRiskSyncCommand:
    request: RiskRequest


@dataclass(frozen=True)
class EnqueueRiskAnalysisCommand:
    request: RiskRequest


@dataclass(frozen=True)
class StartRiskAnalysisCommand:
    job_id: str


@dataclass(frozen=True)
class CompleteRiskAnalysisCommand:
    job_id: str
    result: RiskResponse


@dataclass(frozen=True)
class FailRiskAnalysisCommand:
    job_id: str
    error: str
