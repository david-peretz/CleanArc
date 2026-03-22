from models.risk_models import RiskRequest, RiskResponse
from tools.statistical_tools import StatisticalRiskTools


class RiskAgentService:
    def analyze(self, request: RiskRequest) -> RiskResponse:
        score = StatisticalRiskTools.combined_score(request.age, request.claims, request.amount)

        if score > 0.7:
            reason = "High claim amount combined with elevated claim frequency"
        elif score >= 0.4:
            reason = "Moderate risk profile requiring manual underwriter review"
        else:
            reason = "Low risk profile based on age, claim history, and amount"

        return RiskResponse(score=round(score, 2), reason=reason)
