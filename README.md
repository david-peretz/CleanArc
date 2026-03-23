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

## Queue / Cache / Fallback / Orchestration
The Python service now includes explicit runtime orchestration patterns:

- `Queue`:
  - Async jobs are enqueued and processed by a background worker.
  - Endpoints:
    - `POST /api/risk/analyze/async` -> returns `job_id`
    - `GET /api/risk/jobs/{job_id}` -> returns job status/result

- `Cache`:
  - In-memory TTL cache for repeated requests (`age/claims/amount` key).
  - Cache TTL is configurable via `RISK_CACHE_TTL_SECONDS` (default: `300`).

- `Fallback`:
  - If LLM orchestration is unavailable/fails, deterministic logic is used.
  - Response includes `source` metadata: `cache` / `llm` / `fallback`.

- `Orchestration`:
  - Sync path: immediate response from `POST /api/risk/analyze`
  - Async path: queued execution + polling by job id
  - Worker lifecycle is attached to FastAPI startup/shutdown events.

## Endpoints
- `POST /api/risk`
- `POST /api/risk/analyze` (sync)
- `POST /api/risk/analyze/async` (enqueue async job)
- `GET /api/risk/jobs/{job_id}` (poll async job status)
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

## React Client (Risk UI)
A lightweight React client is served directly by `CleanArc.WebApi` as static files.

Location:
- `CleanArc.WebApi/wwwroot/risk-ui/index.html`

Capabilities:
- Input form for `age`, `claims`, `amount`
- Result display for `score`, `decision`, `reason`
- Decision history (stored in browser `localStorage`)
- Explainability section (with specific notes for reject outcomes)
- Charts:
  - Score trend over time
  - Decision distribution (`Approve` / `Review` / `Reject`)

Run and open:
1. Start Python service (port `8000`)
2. Start .NET API (`dotnet run --project CleanArc.WebApi --launch-profile http`)
3. Open `http://localhost:5149/risk-ui/`

## Configuration
`CleanArc.WebApi/appsettings*.json` contains:
- `PythonRiskService:BaseUrl`
- `PythonRiskService:AnalyzePath`
- `PythonRiskService:TimeoutSeconds`
- `PythonRiskService:MaxRetries`

## Notes
This implementation is production-oriented scaffolding: clean separation of concerns, deterministic decisions, and safe AI integration.

## Reliability Principles
- AI is an advisory layer, not an authority.
- Deterministic guardrails are enforced above the model.
- The system is designed to degrade gracefully when AI components are unavailable.
