from __future__ import annotations

from orchestration.commands import (
    CompleteRiskAnalysisCommand,
    EnqueueRiskAnalysisCommand,
    FailRiskAnalysisCommand,
    StartRiskAnalysisCommand,
)
from orchestration.saga_state import SagaState
from orchestration.store import SagaStateStore


class RiskCommandHandlers:
    def __init__(self, store: SagaStateStore) -> None:
        self._store = store

    async def handle_enqueue(self, command: EnqueueRiskAnalysisCommand, job_id: str) -> SagaState:
        now = SagaState.now_utc()
        state = SagaState(
            job_id=job_id,
            status="queued",
            request=command.request,
            created_at_utc=now,
            updated_at_utc=now,
        )
        await self._store.save(state)
        return state

    async def handle_start(self, command: StartRiskAnalysisCommand) -> SagaState:
        state = await self._require_state(command.job_id)
        state.status = "running"
        state.updated_at_utc = SagaState.now_utc()
        await self._store.save(state)
        return state

    async def handle_complete(self, command: CompleteRiskAnalysisCommand) -> SagaState:
        state = await self._require_state(command.job_id)
        state.status = "completed"
        state.result = command.result
        state.error = None
        state.updated_at_utc = SagaState.now_utc()
        await self._store.save(state)
        return state

    async def handle_fail(self, command: FailRiskAnalysisCommand) -> SagaState:
        state = await self._require_state(command.job_id)
        state.status = "failed"
        state.error = command.error
        state.updated_at_utc = SagaState.now_utc()
        await self._store.save(state)
        return state

    async def _require_state(self, job_id: str) -> SagaState:
        state = await self._store.get(job_id)
        if state is None:
            raise ValueError(f"Saga state for job '{job_id}' was not found")
        return state
