import asyncio
import json
import os
from copy import deepcopy
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from typing import Any, Dict
from uuid import uuid4

from models.risk_models import RiskJobStatusResponse, RiskRequest, RiskResponse
from tools.statistical_tools import StatisticalRiskTools


@dataclass
class _CacheEntry:
    response: RiskResponse
    expires_at: datetime


class RiskAgentService:
    def __init__(self) -> None:
        self._llm = self._build_llm()
        self._cache_ttl_seconds = int(os.getenv("RISK_CACHE_TTL_SECONDS", "300"))
        self._cache: dict[str, _CacheEntry] = {}

        self._jobs: dict[str, RiskJobStatusResponse] = {}
        self._queue: asyncio.Queue[str] = asyncio.Queue()
        self._worker_task: asyncio.Task | None = None
        self._jobs_lock = asyncio.Lock()

    async def start_worker(self) -> None:
        if self._worker_task is None or self._worker_task.done():
            self._worker_task = asyncio.create_task(self._worker_loop(), name="risk-worker")

    async def stop_worker(self) -> None:
        if self._worker_task is None:
            return

        self._worker_task.cancel()
        try:
            await self._worker_task
        except asyncio.CancelledError:
            pass
        finally:
            self._worker_task = None

    async def enqueue_analysis(self, request: RiskRequest) -> str:
        job_id = str(uuid4())
        job = RiskJobStatusResponse(job_id=job_id, status="queued")

        async with self._jobs_lock:
            self._jobs[job_id] = job
            setattr(self._jobs[job_id], "_request", request)

        await self._queue.put(job_id)
        return job_id

    async def get_job_status(self, job_id: str) -> RiskJobStatusResponse | None:
        async with self._jobs_lock:
            job = self._jobs.get(job_id)
            if job is None:
                return None
            return deepcopy(job)

    def analyze(self, request: RiskRequest) -> RiskResponse:
        return self.analyze_sync(request)

    def analyze_sync(self, request: RiskRequest) -> RiskResponse:
        key = self._request_key(request)
        cached = self._try_get_cache(key)
        if cached is not None:
            cached.source = "cache"
            return cached

        score = StatisticalRiskTools.combined_score(request.age, request.claims, request.amount)
        fallback = self._deterministic_decision(score)

        if self._llm is None:
            response = RiskResponse(
                score=round(score, 2),
                decision=fallback["decision"],
                reason=fallback["reason"],
                source="fallback",
            )
            self._store_cache(key, response)
            return response

        dynamic = self._dynamic_decision(request, score)
        if dynamic is None:
            response = RiskResponse(
                score=round(score, 2),
                decision=fallback["decision"],
                reason=fallback["reason"],
                source="fallback",
            )
            self._store_cache(key, response)
            return response

        response = RiskResponse(
            score=round(score, 2),
            decision=dynamic["decision"],
            reason=dynamic["reason"],
            source="llm",
        )
        self._store_cache(key, response)
        return response

    async def _worker_loop(self) -> None:
        while True:
            job_id = await self._queue.get()
            try:
                async with self._jobs_lock:
                    job = self._jobs.get(job_id)
                    if job is None:
                        continue

                    job.status = "running"
                    request = getattr(job, "_request", None)

                if request is None:
                    raise ValueError("Missing request payload for queued job.")

                result = await asyncio.to_thread(self.analyze_sync, request)

                async with self._jobs_lock:
                    current = self._jobs.get(job_id)
                    if current is not None:
                        current.status = "completed"
                        current.result = result
                        current.error = None
            except Exception as ex:
                async with self._jobs_lock:
                    current = self._jobs.get(job_id)
                    if current is not None:
                        current.status = "failed"
                        current.error = str(ex)
            finally:
                async with self._jobs_lock:
                    current = self._jobs.get(job_id)
                    if current is not None and hasattr(current, "_request"):
                        delattr(current, "_request")

                self._queue.task_done()

    def _request_key(self, request: RiskRequest) -> str:
        return f"{request.age}:{request.claims}:{round(float(request.amount), 2)}"

    def _try_get_cache(self, key: str) -> RiskResponse | None:
        entry = self._cache.get(key)
        if entry is None:
            return None

        now = datetime.now(timezone.utc)
        if entry.expires_at <= now:
            self._cache.pop(key, None)
            return None

        return deepcopy(entry.response)

    def _store_cache(self, key: str, response: RiskResponse) -> None:
        ttl = max(1, self._cache_ttl_seconds)
        expires_at = datetime.now(timezone.utc) + timedelta(seconds=ttl)
        self._cache[key] = _CacheEntry(response=deepcopy(response), expires_at=expires_at)

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
        if score >= 0.4:
            return {
                "decision": "Review",
                "reason": "Moderate risk profile requiring manual underwriter review",
            }

        return {
            "decision": "Approve",
            "reason": "Low risk profile based on age, claim history, and amount",
        }
