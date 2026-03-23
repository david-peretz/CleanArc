from fastapi import FastAPI

from api.routes import (
    router as risk_router,
    start_background_worker,
    stop_background_worker,
)

app = FastAPI(title="AI Risk Analysis Service", version="1.0.0")
app.include_router(risk_router)


@app.on_event("startup")
async def on_startup() -> None:
    await start_background_worker()


@app.on_event("shutdown")
async def on_shutdown() -> None:
    await stop_background_worker()
