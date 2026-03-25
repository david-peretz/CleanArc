from fastapi import APIRouter, HTTPException, status

from models.risk_models import RiskJobAcceptedResponse, RiskJobStatusResponse, RiskRequest, RiskResponse
from orchestration.commands import AnalyzeRiskSyncCommand, EnqueueRiskAnalysisCommand
from orchestration.orchestrator import RiskOrchestrator
from orchestration.store import build_saga_store
from services.risk_agent_service import RiskAgentService

router = APIRouter(prefix="/api/risk", tags=["risk"])
risk_service = RiskAgentService()
orchestrator = RiskOrchestrator(risk_service=risk_service, store=build_saga_store())


@router.post("/analyze", response_model=RiskResponse)
async def analyze_risk(request: RiskRequest) -> RiskResponse:
    return await orchestrator.analyze_sync(AnalyzeRiskSyncCommand(request=request))


@router.post("/analyze/async", response_model=RiskJobAcceptedResponse, status_code=status.HTTP_202_ACCEPTED)
async def analyze_risk_async(request: RiskRequest) -> RiskJobAcceptedResponse:
    saga = await orchestrator.enqueue(EnqueueRiskAnalysisCommand(request=request))
    return RiskJobAcceptedResponse(job_id=saga.job_id, status="queued")


@router.get("/jobs/{job_id}", response_model=RiskJobStatusResponse)
async def get_risk_job(job_id: str) -> RiskJobStatusResponse:
    saga = await orchestrator.get_saga(job_id)
    if saga is None:
        raise HTTPException(status_code=404, detail=f"Job '{job_id}' was not found.")
    return RiskJobStatusResponse(
        job_id=saga.job_id,
        status=saga.status,
        result=saga.result,
        error=saga.error,
    )


async def start_background_worker() -> None:
    await orchestrator.start()


async def stop_background_worker() -> None:
    await orchestrator.stop()
