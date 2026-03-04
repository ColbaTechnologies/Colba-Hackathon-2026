"""
Message Entity - Dominio
Representa un mensaje en el sistema de mensajería asíncrona
"""
from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional, Dict, Any
from uuid import UUID, uuid4
from enum import Enum

class MessageStatus(str, Enum):
    """Estados posibles de un mensaje"""
    PENDING = "pending"
    PROCESSING = "processing"
    DELIVERED = "delivered"
    FAILED = "failed"
    SCHEDULED = "scheduled"


@dataclass
class Message:
    """Entidad Message del dominio"""
    destination_url: str
    payload: Dict[str, Any]
    id: UUID = field(default_factory=uuid4)
    status: MessageStatus = MessageStatus.PENDING
    retry_count: int = 0
    max_retries: int = 3
    scheduled_at: Optional[datetime] = None
    created_at: Optional[datetime] = None
    updated_at: Optional[datetime] = None
    delivered_at: Optional[datetime] = None
    last_error: Optional[str] = None
    headers: Optional[Dict[str, str]] = None