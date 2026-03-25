from __future__ import annotations

from datetime import datetime, timezone
from typing import Literal

from pydantic import BaseModel

from models.risk_models import RiskRequest, RiskResponse

SagaStatus = Literal["queued", "running", "completed", "failed"]


class SagaState(BaseModel):
    job_id: str
    status: SagaStatus
    request: RiskRequest
    result: RiskResponse | None = None
    error: str | None = None
    created_at_utc: datetime
    updated_at_utc: datetime

    @staticmethod
    def now_utc() -> datetime:
        return datetime.now(timezone.utc)
