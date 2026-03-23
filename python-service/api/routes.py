from fastapi import APIRouter, HTTPException, status

from models.risk_models import RiskJobAcceptedResponse, RiskJobStatusResponse, RiskRequest, RiskResponse
from services.risk_agent_service import RiskAgentService

router = APIRouter(prefix="/api/risk", tags=["risk"])
agent_service = RiskAgentService()


@router.post("/analyze", response_model=RiskResponse)
def analyze_risk(request: RiskRequest) -> RiskResponse:
    return agent_service.analyze_sync(request)


@router.post("/analyze/async", response_model=RiskJobAcceptedResponse, status_code=status.HTTP_202_ACCEPTED)
async def analyze_risk_async(request: RiskRequest) -> RiskJobAcceptedResponse:
    job_id = await agent_service.enqueue_analysis(request)
    return RiskJobAcceptedResponse(job_id=job_id, status="queued")


@router.get("/jobs/{job_id}", response_model=RiskJobStatusResponse)
async def get_risk_job(job_id: str) -> RiskJobStatusResponse:
    job = await agent_service.get_job_status(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail=f"Job '{job_id}' was not found.")
    return job


async def start_background_worker() -> None:
    await agent_service.start_worker()


async def stop_background_worker() -> None:
    await agent_service.stop_worker()
