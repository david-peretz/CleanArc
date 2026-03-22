# AI Risk Analysis System

Clean Architecture with .NET Core + Python AI Agent.

## Overview
This solution keeps a clean layered architecture in .NET and adds a Python AI service for insurance risk analysis.

The API receives customer profile data and returns:
- Risk score
- Deterministic business decision (`Approve` / `Review` / `Reject`)
- Human-readable explanation

## Architecture
- `CleanArc.Domain`: domain rules and enums (including risk decision enum).
- `CleanArc.Application`: use-cases and interfaces (`InsuranceRiskAnalysisService`, `IPythonRiskAgentClient`).
- `CleanArc.Infrastructure`: external integrations (Python HTTP client, retries, resiliency).
- `CleanArc.WebApi`: REST controllers (`POST /api/risk`).
- `python-service/`: FastAPI service with deterministic statistical tooling.

## Decision Rules
Final business decision is deterministic in .NET (not delegated to AI):
- `score > 0.7` -> `Reject`
- `0.4 <= score <= 0.7` -> `Review`
- `score < 0.4` -> `Approve`

## Guardrails
- Input validation before Python call
- Output validation after Python call (`score` must be `[0,1]`, reason required)
- Retry policy for external AI service calls

## Endpoints
- `POST /api/risk`
- Existing municipal endpoints remain available and unchanged.

Example request:
```json
{
  "age": 45,
  "claims": 3,
  "amount": 12000
}
```

Example response:
```json
{
  "score": 0.68,
  "decision": "Review",
  "reason": "Moderate risk profile requiring manual underwriter review"
}
```

## Run Locally
### 1) Python service
```bash
cd python-service
pip install -r requirements.txt
uvicorn main:app --reload --port 8000
```

### 2) .NET API
```bash
dotnet restore CleanArcInterview.sln
dotnet run --project CleanArc.WebApi
```

### 3) Test
`POST http://localhost:5000/api/risk`

## Configuration
`CleanArc.WebApi/appsettings*.json` contains:
- `PythonRiskService:BaseUrl`
- `PythonRiskService:AnalyzePath`
- `PythonRiskService:TimeoutSeconds`
- `PythonRiskService:MaxRetries`

## Notes
This implementation is production-oriented scaffolding: clean separation of concerns, deterministic decisions, and safe AI integration.
