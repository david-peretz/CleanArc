from fastapi import FastAPI

from api.routes import router as risk_router

app = FastAPI(title="AI Risk Analysis Service", version="1.0.0")
app.include_router(risk_router)
