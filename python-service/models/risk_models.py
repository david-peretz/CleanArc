from pydantic import BaseModel, Field


class RiskRequest(BaseModel):
    age: int = Field(ge=18, le=120)
    claims: int = Field(ge=0, le=100)
    amount: float = Field(ge=0)


class RiskResponse(BaseModel):
    score: float = Field(ge=0, le=1)
    reason: str
