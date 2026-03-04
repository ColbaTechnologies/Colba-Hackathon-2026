"""
API Models (DTOs)
Modelos de datos para la API
"""
from pydantic import BaseModel, Field, HttpUrl
from typing import Optional, Dict, Any
from datetime import datetime
from uuid import UUID


class CreateMessageRequest(BaseModel):
    """Request para crear un mensaje"""
    destination_url: str = Field(..., description="URL de destino")
    payload: Dict[str, Any] = Field(..., description="Cuerpo del mensaje")
    scheduled_at: Optional[datetime] = Field(None, description="Fecha/hora programada")
    headers: Optional[Dict[str, str]] = Field(None, description="Headers personalizados")
    max_retries: int = Field(3, ge=0, le=10, description="Número máximo de reintentos")


class MessageResponse(BaseModel):
    """Response con información del mensaje"""
    id: UUID
    destination_url: str
    payload: Dict[str, Any]
    status: str
    retry_count: int
    max_retries: int
    scheduled_at: Optional[datetime] = None
    created_at: datetime
    updated_at: datetime
    delivered_at: Optional[datetime] = None
    last_error: Optional[str] = None


class MessageCreatedResponse(BaseModel):
    """Response al crear un mensaje"""
    message_id: UUID
    status: str
    message: str = "Message created successfully"
