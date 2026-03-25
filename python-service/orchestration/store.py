from __future__ import annotations

import abc
import asyncio
import json
import os
import sqlite3
from typing import Iterable

from orchestration.saga_state import SagaState


class SagaStateStore(abc.ABC):
    @abc.abstractmethod
    async def initialize(self) -> None:
        raise NotImplementedError

    @abc.abstractmethod
    async def save(self, state: SagaState) -> None:
        raise NotImplementedError

    @abc.abstractmethod
    async def get(self, job_id: str) -> SagaState | None:
        raise NotImplementedError

    @abc.abstractmethod
    async def list_by_status(self, statuses: Iterable[str]) -> list[SagaState]:
        raise NotImplementedError


class SqliteSagaStateStore(SagaStateStore):
    def __init__(self, db_path: str) -> None:
        self._db_path = db_path

    async def initialize(self) -> None:
        await asyncio.to_thread(self._initialize_sync)

    async def save(self, state: SagaState) -> None:
        await asyncio.to_thread(self._save_sync, state)

    async def get(self, job_id: str) -> SagaState | None:
        return await asyncio.to_thread(self._get_sync, job_id)

    async def list_by_status(self, statuses: Iterable[str]) -> list[SagaState]:
        return await asyncio.to_thread(self._list_by_status_sync, list(statuses))

    def _initialize_sync(self) -> None:
        conn = sqlite3.connect(self._db_path)
        try:
            conn.execute(
                """
                CREATE TABLE IF NOT EXISTS saga_state (
                    job_id TEXT PRIMARY KEY,
                    status TEXT NOT NULL,
                    payload_json TEXT NOT NULL,
                    updated_at_utc TEXT NOT NULL
                )
                """
            )
            conn.commit()
        finally:
            conn.close()

    def _save_sync(self, state: SagaState) -> None:
        conn = sqlite3.connect(self._db_path)
        try:
            conn.execute(
                """
                INSERT INTO saga_state (job_id, status, payload_json, updated_at_utc)
                VALUES (?, ?, ?, ?)
                ON CONFLICT(job_id) DO UPDATE SET
                    status=excluded.status,
                    payload_json=excluded.payload_json,
                    updated_at_utc=excluded.updated_at_utc
                """,
                (
                    state.job_id,
                    state.status,
                    state.model_dump_json(),
                    state.updated_at_utc.isoformat(),
                ),
            )
            conn.commit()
        finally:
            conn.close()

    def _get_sync(self, job_id: str) -> SagaState | None:
        conn = sqlite3.connect(self._db_path)
        try:
            row = conn.execute(
                "SELECT payload_json FROM saga_state WHERE job_id = ?",
                (job_id,),
            ).fetchone()
        finally:
            conn.close()

        if row is None:
            return None

        return SagaState.model_validate_json(row[0])

    def _list_by_status_sync(self, statuses: list[str]) -> list[SagaState]:
        if not statuses:
            return []

        placeholders = ", ".join("?" for _ in statuses)
        conn = sqlite3.connect(self._db_path)
        try:
            rows = conn.execute(
                f"SELECT payload_json FROM saga_state WHERE status IN ({placeholders})",
                tuple(statuses),
            ).fetchall()
        finally:
            conn.close()

        return [SagaState.model_validate_json(row[0]) for row in rows]


class RedisSagaStateStore(SagaStateStore):
    _STATUSES = ("queued", "running", "completed", "failed")

    def __init__(self, redis_url: str) -> None:
        from redis.asyncio import Redis

        self._redis_url = redis_url
        self._redis: Redis | None = None

    async def initialize(self) -> None:
        from redis.asyncio import Redis

        self._redis = Redis.from_url(self._redis_url, decode_responses=True)

    async def save(self, state: SagaState) -> None:
        redis = self._require_client()
        key = self._key(state.job_id)

        payload = state.model_dump_json()
        async with redis.pipeline(transaction=True) as pipe:
            for status in self._STATUSES:
                pipe.srem(self._status_key(status), state.job_id)
            pipe.set(key, payload)
            pipe.sadd(self._status_key(state.status), state.job_id)
            await pipe.execute()

    async def get(self, job_id: str) -> SagaState | None:
        redis = self._require_client()
        raw = await redis.get(self._key(job_id))
        if raw is None:
            return None
        return SagaState.model_validate_json(raw)

    async def list_by_status(self, statuses: Iterable[str]) -> list[SagaState]:
        redis = self._require_client()

        ids: set[str] = set()
        for status in statuses:
            ids.update(await redis.smembers(self._status_key(status)))

        if not ids:
            return []

        raw_values = await redis.mget([self._key(job_id) for job_id in ids])
        return [SagaState.model_validate_json(raw) for raw in raw_values if raw]

    def _require_client(self):
        if self._redis is None:
            raise RuntimeError("Redis saga store not initialized")
        return self._redis

    @staticmethod
    def _key(job_id: str) -> str:
        return f"risk:saga:{job_id}"

    @staticmethod
    def _status_key(status: str) -> str:
        return f"risk:saga:status:{status}"


def build_saga_store() -> SagaStateStore:
    redis_url = os.getenv("REDIS_URL", "").strip()
    if redis_url:
        return RedisSagaStateStore(redis_url)

    db_path = os.getenv("SAGA_DB_PATH", "python-service/.risk_saga.db")
    return SqliteSagaStateStore(db_path)
