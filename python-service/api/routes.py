from fastapi import APIRouter

from models.risk_models import RiskRequest, RiskResponse
from services.risk_agent_service import RiskAgentService

router = APIRouter(prefix="/api/risk", tags=["risk"])
agent_service = RiskAgentService()


@router.post("/analyze", response_model=RiskResponse)
def analyze_risk(request: RiskRequest) -> RiskResponse:
    return agent_service.analyze(request)
