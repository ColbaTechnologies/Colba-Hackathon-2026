from typing import List, Optional
from uuid import UUID
from datetime import datetime, timedelta
import json
import logging

from domain.entities.message import Message, MessageStatus
from domain.repositories.message import MessageRepository
from infraestructure.database.postgres_datasource import PostgresDataSource

logger = logging.getLogger(__name__)


class PostgresMessageRepository(MessageRepository):
    """Implementación PostgreSQL del repositorio de mensajes"""

    def __init__(self, datasource: PostgresDataSource):
        self.datasource = datasource

    async def save(self, message: Message) -> Message:
        """Guarda un nuevo mensaje en la base de datos"""
        query = """
                INSERT INTO messages (
                    id, destination_url, payload, status, retry_count,
                    max_retries, scheduled_at, headers
                )
                VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
                    RETURNING created_at, updated_at \
                """
        try:
            row = await self.datasource.fetchrow(
                query,
                message.id,
                message.destination_url,
                json.dumps(message.payload),
                message.status.value,
                message.retry_count,
                message.max_retries,
                message.scheduled_at,
                json.dumps(message.headers) if message.headers else None
            )
            message.created_at = row['created_at']
            message.updated_at = row['updated_at']
            logger.info(f"Message saved: {message.id}")
            return message
        except Exception as e:
            logger.error(f"Error saving message: {e}")
            raise

    async def find_by_id(self, message_id: UUID) -> Optional[Message]:
        """Busca un mensaje por su ID"""
        query = "SELECT * FROM messages WHERE id = $1"
        try:
            row = await self.datasource.fetchrow(query, message_id)
            return self._row_to_message(row) if row else None
        except Exception as e:
            logger.error(f"Error finding message by id {message_id}: {e}")
            raise

    async def update(self, message: Message) -> Message:
        """Actualiza un mensaje existente"""
        query = """
                UPDATE messages
                SET status = $1,
                    retry_count = $2,
                    last_error = $3,
                    delivered_at = $4,
                    updated_at = NOW()
                WHERE id = $5
                    RETURNING updated_at \
                """
        try:
            row = await self.datasource.fetchrow(
                query,
                message.status.value,
                message.retry_count,
                message.last_error,
                message.delivered_at,
                message.id
            )
            if row:
                message.updated_at = row['updated_at']
                logger.info(f"Message updated: {message.id}")
            return message
        except Exception as e:
            logger.error(f"Error updating message {message.id}: {e}")
            raise

    async def delete(self, message_id: UUID) -> bool:
        """Elimina un mensaje (dead letter)"""
        query = "DELETE FROM messages WHERE id = $1"
        try:
            result = await self.datasource.execute(query, message_id)
            deleted = result == "DELETE 1"
            if deleted:
                logger.info(f"Message deleted: {message_id}")
            return deleted
        except Exception as e:
            logger.error(f"Error deleting message {message_id}: {e}")
            raise

    def _row_to_message(self, row) -> Message:
        """Convierte una fila de la BD a una entidad Message"""
        return Message(
            id=row['id'],
            destination_url=row['destination_url'],
            payload=json.loads(row['payload']) if isinstance(row['payload'], str) else row['payload'],
            status=MessageStatus(row['status']),
            retry_count=row['retry_count'],
            max_retries=row['max_retries'],
            scheduled_at=row['scheduled_at'],
            created_at=row['created_at'],
            updated_at=row['updated_at'],
            delivered_at=row['delivered_at'],
            last_error=row['last_error'],
            headers=json.loads(row['headers']) if row['headers'] else None
        )

    async def fetch_next_pending(self, now: datetime) -> Optional[Message]:
        """Obtiene y marca como en proceso el siguiente mensaje pendiente"""
        query = """
                SELECT * FROM messages
                WHERE status IN ('pending', 'scheduled')
                  AND (scheduled_at IS NULL OR scheduled_at <= $1)
                ORDER BY created_at ASC
                FOR UPDATE SKIP LOCKED
                LIMIT 1
                """
        async with self.datasource.get_connection() as conn:
            async with conn.transaction():
                row = await conn.fetchrow(query, now)
                if not row:
                    return None
                await conn.execute(
                    """
                    UPDATE messages
                    SET status = $1,
                        updated_at = NOW()
                    WHERE id = $2
                    """,
                    MessageStatus.PROCESSING.value,
                    row['id']
                )
                row_dict = dict(row)
                row_dict['status'] = MessageStatus.PROCESSING.value
                return self._row_to_message(row_dict)

    async def mark_delivered(self, message_id: UUID) -> None:
        """Marca un mensaje como entregado"""
        query = """
                UPDATE messages
                SET status = $1,
                    delivered_at = NOW(),
                    last_error = NULL,
                    updated_at = NOW()
                WHERE id = $2
                """
        await self.datasource.execute(query, MessageStatus.DELIVERED.value, message_id)
        logger.info(f"Message delivered: {message_id}")

    async def mark_failed(self, message: Message, error: str, retry_delay_seconds: int) -> str:
        """Marca un mensaje como fallido o reprogramado"""
        next_retry_count = message.retry_count + 1
        should_retry = next_retry_count <= message.max_retries
        if should_retry:
            status = MessageStatus.SCHEDULED
            scheduled_at = datetime.utcnow() + timedelta(seconds=retry_delay_seconds)
        else:
            status = MessageStatus.FAILED
            scheduled_at = None

        query = """
                UPDATE messages
                SET status = $1,
                    retry_count = $2,
                    last_error = $3,
                    scheduled_at = $4,
                    updated_at = NOW()
                WHERE id = $5
                """
        await self.datasource.execute(
            query,
            status.value,
            next_retry_count,
            error,
            scheduled_at,
            message.id
        )
        logger.warning(f"Message {status.value}: {message.id} - {error}")
        return status.value