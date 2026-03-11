import asyncio
import logging
from datetime import datetime
from typing import Optional

import httpx

from domain.entities.message import Message
from domain.repositories.message import MessageRepository

logger = logging.getLogger(__name__)

# TODO @HACKATHON - this should be use case and the worker call it
class MessageListener:
    """Worker que extrae mensajes de la BD y los entrega a su destino"""

    def __init__(
            self,
            repository: MessageRepository,
            poll_interval_seconds: float,
            http_timeout_seconds: float,
            retry_delay_seconds: int,
            enabled: bool = True
    ):
        self.repository = repository
        self.poll_interval_seconds = poll_interval_seconds
        self.http_timeout_seconds = http_timeout_seconds
        self.retry_delay_seconds = retry_delay_seconds
        self.enabled = enabled
        self._stop_event = asyncio.Event()
        self._task: Optional[asyncio.Task] = None

    async def start(self) -> None:
        if not self.enabled:
            logger.info("Message listener disabled")
            return
        if self._task and not self._task.done():
            return
        self._stop_event.clear()
        self._task = asyncio.create_task(self._run())
        logger.info("Message listener started")

    async def stop(self) -> None:
        if not self._task:
            return
        self._stop_event.set()
        await self._task
        logger.info("Message listener stopped")

    async def _run(self) -> None:
        timeout = httpx.Timeout(self.http_timeout_seconds)
        async with httpx.AsyncClient(timeout=timeout) as client:
            while not self._stop_event.is_set():
                message = await self.repository.fetch_next_pending(datetime.utcnow())
                if not message:
                    await self._wait_for_stop(self.poll_interval_seconds)
                    continue
                await self._deliver(message, client)

    async def _wait_for_stop(self, timeout_seconds: float) -> None:
        try:
            await asyncio.wait_for(self._stop_event.wait(), timeout=timeout_seconds)
        except asyncio.TimeoutError:
            return

    async def _deliver(self, message: Message, client: httpx.AsyncClient) -> None:
        try:
            response = await client.post(
                message.destination_url,
                json=message.payload,
                headers=message.headers or {}
            )
            if 200 <= response.status_code < 300:
                await self.repository.mark_delivered(message.id)
            else:
                error = f"HTTP {response.status_code}: {response.text[:500]}"
                await self.repository.mark_failed(message, error, self.retry_delay_seconds)
        except Exception as exc:
            await self.repository.mark_failed(message, str(exc), self.retry_delay_seconds)