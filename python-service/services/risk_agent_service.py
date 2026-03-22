from models.risk_models import RiskRequest, RiskResponse
from tools.statistical_tools import StatisticalRiskTools


class RiskAgentService:
    def analyze(self, request: RiskRequest) -> RiskResponse:
        score = StatisticalRiskTools.combined_score(request.age, request.claims, request.amount)

        if score > 0.7:
            decision = "Reject"
            reason = "High claim amount combined with elevated claim frequency"
        elif score >= 0.4:
            decision = "Review"
            reason = "Moderate risk profile requiring manual underwriter review"
        else:
            decision = "Approve"
            reason = "Low risk profile based on age, claim history, and amount"

        return RiskResponse(score=round(score, 2), decision=decision, reason=reason)
