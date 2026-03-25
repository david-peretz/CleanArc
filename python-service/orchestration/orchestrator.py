from __future__ import annotations

import asyncio
from uuid import uuid4

from models.risk_models import RiskRequest, RiskResponse
from orchestration.commands import (
    AnalyzeRiskSyncCommand,
    CompleteRiskAnalysisCommand,
    EnqueueRiskAnalysisCommand,
    FailRiskAnalysisCommand,
    StartRiskAnalysisCommand,
)
from orchestration.handlers import RiskCommandHandlers
from orchestration.saga_state import SagaState
from orchestration.store import SagaStateStore
from services.risk_agent_service import RiskAgentService


class RiskOrchestrator:
    def __init__(self, risk_service: RiskAgentService, store: SagaStateStore) -> None:
        self._risk_service = risk_service
        self._store = store
        self._handlers = RiskCommandHandlers(store)
        self._queue: asyncio.Queue[str] = asyncio.Queue()
        self._worker_task: asyncio.Task | None = None

    async def start(self) -> None:
        await self._store.initialize()
        await self._rehydrate_queue()

        if self._worker_task is None or self._worker_task.done():
            self._worker_task = asyncio.create_task(self._worker_loop(), name="risk-orchestrator-worker")

    async def stop(self) -> None:
        if self._worker_task is None:
            return

        self._worker_task.cancel()
        try:
            await self._worker_task
        except asyncio.CancelledError:
            pass
        finally:
            self._worker_task = None

    async def analyze_sync(self, command: AnalyzeRiskSyncCommand) -> RiskResponse:
        # Stateless sync path: no in-memory saga mutation, direct command execution.
        return await asyncio.to_thread(self._risk_service.analyze_sync, command.request)

    async def enqueue(self, command: EnqueueRiskAnalysisCommand) -> SagaState:
        job_id = str(uuid4())
        state = await self._handlers.handle_enqueue(command, job_id)
        await self._queue.put(job_id)
        return state

    async def get_saga(self, job_id: str) -> SagaState | None:
        return await self._store.get(job_id)

    async def _rehydrate_queue(self) -> None:
        pending = await self._store.list_by_status(["queued", "running"])
        for state in pending:
            if state.status == "running":
                # If process crashed mid-flight, re-queue as queued.
                state.status = "queued"
                state.updated_at_utc = SagaState.now_utc()
                await self._store.save(state)
            await self._queue.put(state.job_id)

    async def _worker_loop(self) -> None:
        while True:
            job_id = await self._queue.get()
            try:
                await self._handlers.handle_start(StartRiskAnalysisCommand(job_id=job_id))
                state = await self._store.get(job_id)
                if state is None:
                    continue

                result = await asyncio.to_thread(self._risk_service.analyze_sync, state.request)
                await self._handlers.handle_complete(
                    CompleteRiskAnalysisCommand(job_id=job_id, result=result)
                )
            except Exception as ex:
                await self._handlers.handle_fail(
                    FailRiskAnalysisCommand(job_id=job_id, error=str(ex))
                )
            finally:
                self._queue.task_done()
