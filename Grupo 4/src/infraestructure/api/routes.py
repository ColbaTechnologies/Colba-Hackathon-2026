"""
API Routes
Endpoints de la API REST
"""
from fastapi import APIRouter, Depends, HTTPException, status
from uuid import UUID
from typing import List

from .models import (
    CreateMessageRequest,
    MessageResponse,
    MessageCreatedResponse,
)
from application.use_cases.create_message import CreateMessageUseCase
from domain.repositories.message import MessageRepository


# Router
router = APIRouter()


# Dependency injection placeholders (se configurarán en main.py)
def get_message_repository() -> MessageRepository:
    raise NotImplementedError


@router.post("/messages", response_model=MessageCreatedResponse, status_code=status.HTTP_202_ACCEPTED)
async def create_message(
        request: CreateMessageRequest,
        message_repo: MessageRepository = Depends(get_message_repository)
):
    """
    Crea un nuevo mensaje para envío asíncrono

    Retorna 202 Accepted inmediatamente y procesa el mensaje de forma asíncrona
    """
    use_case = CreateMessageUseCase(message_repo)

    message = await use_case.execute(
        destination_url=request.destination_url,
        payload=request.payload,
        scheduled_at=request.scheduled_at,
        headers=request.headers,
        max_retries=request.max_retries
    )

    return MessageCreatedResponse(
        message_id=message.id,
        status=message.status.value
    )


@router.delete("/messages/{message_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_message(
        message_id: UUID,
        message_repo: MessageRepository = Depends(get_message_repository)
):
    """Elimina un mensaje (dead letter)"""
    deleted = await message_repo.delete(message_id)

    if not deleted:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Message not found"
        )


@router.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy"}