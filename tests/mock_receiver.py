from fastapi import FastAPI, Request
import logging

app = FastAPI()

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


@app.post("/receive")
async def receive(request: Request):
    payload = await request.json()
    logger.info(f"Received payload: {payload}")
    return {"received": payload, "headers": dict(request.headers)}