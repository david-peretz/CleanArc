from typing import Literal

from pydantic import BaseModel, Field


class RiskRequest(BaseModel):
    age: int = Field(ge=18, le=120)
    claims: int = Field(ge=0, le=100)
    amount: float = Field(ge=0)


class RiskResponse(BaseModel):
    score: float = Field(ge=0, le=1)
    decision: Literal["Approve", "Review", "Reject"]
    reason: str
    source: Literal["cache", "llm", "fallback"] = "fallback"


class RiskJobAcceptedResponse(BaseModel):
    job_id: str
    status: Literal["queued"]


class RiskJobStatusResponse(BaseModel):
    job_id: str
    status: Literal["queued", "running", "completed", "failed"]
    result: RiskResponse | None = None
    error: str | None = None
