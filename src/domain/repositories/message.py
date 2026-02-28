"""
Message Repository Interface - Dominio
Define el contrato para el repositorio de mensajes
"""
from abc import ABC, abstractmethod
from datetime import datetime
from typing import Optional
from uuid import UUID

from domain.entities.message import Message


class MessageRepository(ABC):
    """Interface del repositorio de mensajes"""

    @abstractmethod
    async def save(self, message: Message) -> Message:
        """Guarda un nuevo mensaje"""
        pass

    @abstractmethod
    async def fetch_next_pending(self, now: datetime) -> Optional[Message]:
        """Obtiene y marca como en proceso el siguiente mensaje pendiente"""
        pass

    @abstractmethod
    async def mark_delivered(self, message_id: UUID) -> None:
        """Marca un mensaje como entregado"""
        pass

    @abstractmethod
    async def mark_failed(self, message: Message, error: str, retry_delay_seconds: int) -> str:
        """Marca un mensaje como fallido o reprogramado"""
        pass