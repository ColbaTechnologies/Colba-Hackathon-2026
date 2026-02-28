"""
Create Message Use Case
Caso de uso para crear y encolar un mensaje
"""
from datetime import datetime
from typing import Optional, Dict, Any
import logging

from domain.entities.message import Message, MessageStatus
from domain.repositories.message import MessageRepository

logger = logging.getLogger(__name__)


class CreateMessageUseCase:
    """Caso de uso para crear mensajes"""

    def __init__(self, message_repository: MessageRepository):
        self.message_repository = message_repository

    async def execute(
            self,
            destination_url: str,
            payload: Dict[str, Any],
            scheduled_at: Optional[datetime] = None,
            headers: Optional[Dict[str, str]] = None,
            max_retries: int = 3
    ) -> Message:
        """
        Crea un nuevo mensaje en el sistema

        Args:
            destination_url: URL de destino
            payload: Cuerpo del mensaje
            scheduled_at: Fecha/hora programada (opcional)
            headers: Headers a enviar (opcional)
            max_retries: Número máximo de reintentos

        Returns:
            Message: El mensaje creado
        """
        try:
            # Determinar el estado inicial
            status = MessageStatus.SCHEDULED if scheduled_at else MessageStatus.PENDING

            # Crear la entidad
            message = Message(
                destination_url=destination_url,
                payload=payload,
                status=status,
                scheduled_at=scheduled_at,
                headers=headers,
                max_retries=max_retries
            )

            # Guardar en el repositorio
            saved_message = await self.message_repository.save(message)

            logger.info(f"Message created successfully: {saved_message.id}")
            return saved_message

        except Exception as e:
            logger.error(f"Error creating message: {e}")
            raise