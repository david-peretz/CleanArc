import json
import os
from typing import Any, Dict

from models.risk_models import RiskRequest, RiskResponse
from tools.statistical_tools import StatisticalRiskTools


class RiskAgentService:
    def __init__(self) -> None:
        self._llm = self._build_llm()

    def analyze(self, request: RiskRequest) -> RiskResponse:
        score = StatisticalRiskTools.combined_score(request.age, request.claims, request.amount)
        fallback = self._deterministic_decision(score)

        if self._llm is None:
            return RiskResponse(score=round(score, 2), decision=fallback["decision"], reason=fallback["reason"])

        dynamic = self._dynamic_decision(request, score)
        if dynamic is None:
            return RiskResponse(score=round(score, 2), decision=fallback["decision"], reason=fallback["reason"])

        return RiskResponse(score=round(score, 2), decision=dynamic["decision"], reason=dynamic["reason"])

    def _dynamic_decision(self, request: RiskRequest, score: float) -> Dict[str, str] | None:
        try:
            from langchain_core.prompts import ChatPromptTemplate

            age_risk = StatisticalRiskTools.age_risk(request.age)
            freq_risk = StatisticalRiskTools.claim_frequency_risk(request.claims)
            amount_risk = StatisticalRiskTools.claim_amount_risk(request.amount)

            prompt = ChatPromptTemplate.from_messages(
                [
                    (
                        "system",
                        "You are an insurance risk decision orchestrator. "
                        "Return strict JSON only with keys: decision, reason. "
                        "decision must be one of Approve, Review, Reject. "
                        "reason must be concise and business-friendly.",
                    ),
                    (
                        "human",
                        "Applicant profile:\n"
                        "- age: {age}\n"
                        "- claims: {claims}\n"
                        "- amount: {amount}\n"
                        "- age_risk: {age_risk}\n"
                        "- frequency_risk: {freq_risk}\n"
                        "- amount_risk: {amount_risk}\n"
                        "- combined_score: {score}\n"
                        "Use this policy guidance:\n"
                        "- score > 0.7 tends to Reject\n"
                        "- 0.4 <= score <= 0.7 tends to Review\n"
                        "- score < 0.4 tends to Approve\n"
                        "You may deviate only if strongly justified by risk components.\n"
                        "Respond with JSON only.",
                    ),
                ]
            )

            chain = prompt | self._llm
            response = chain.invoke(
                {
                    "age": request.age,
                    "claims": request.claims,
                    "amount": request.amount,
                    "age_risk": round(age_risk, 3),
                    "freq_risk": round(freq_risk, 3),
                    "amount_risk": round(amount_risk, 3),
                    "score": round(score, 3),
                }
            )

            payload = self._parse_json_response(getattr(response, "content", ""))
            if payload is None:
                return None

            decision = str(payload.get("decision", "")).strip().title()
            reason = str(payload.get("reason", "")).strip()

            if decision not in {"Approve", "Review", "Reject"} or not reason:
                return None

            return {"decision": decision, "reason": reason}
        except Exception:
            return None

    @staticmethod
    def _parse_json_response(content: str) -> Dict[str, Any] | None:
        text = content.strip()
        if text.startswith("```"):
            text = text.strip("`")
            if text.lower().startswith("json"):
                text = text[4:].strip()

        try:
            parsed = json.loads(text)
        except json.JSONDecodeError:
            return None

        if not isinstance(parsed, dict):
            return None

        return parsed

    @staticmethod
    def _build_llm():
        if not os.getenv("OPENAI_API_KEY"):
            return None

        try:
            from langchain_openai import ChatOpenAI

            model = os.getenv("OPENAI_MODEL", "gpt-4o-mini")
            return ChatOpenAI(model=model, temperature=0)
        except Exception:
            return None

    @staticmethod
    def _deterministic_decision(score: float) -> Dict[str, str]:
        if score > 0.7:
            return {
                "decision": "Reject",
                "reason": "High claim amount combined with elevated claim frequency",
            }
        elif score >= 0.4:
            return {
                "decision": "Review",
                "reason": "Moderate risk profile requiring manual underwriter review",
            }

        return {
            "decision": "Approve",
            "reason": "Low risk profile based on age, claim history, and amount",
        }
