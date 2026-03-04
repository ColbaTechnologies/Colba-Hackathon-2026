import logging
from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

import infraestructure.api.routes as routes
from config import Config
from infraestructure.database.message_repository import PostgresMessageRepository
from infraestructure.database.postgres_datasource import PostgresDataSource
from listener import MessageListener

app = FastAPI()

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

datasource: PostgresDataSource = None
message_repository: PostgresMessageRepository = None

config = Config()

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Maneja el ciclo de vida de la aplicación"""
    global datasource, message_repository, listener

    # Startup
    logger.info("Starting application...")

    # Inicializar datasource
    datasource = PostgresDataSource(
        connection_string=config.database_url,
        min_size=config.DB_POOL_MIN_SIZE,
        max_size=config.DB_POOL_MAX_SIZE
    )
    await datasource.initialize()

    # Inicializar repositorios
    logger.info(f"Creating message repository using datasource {datasource}")
    message_repository = PostgresMessageRepository(datasource)

    listener = MessageListener(
        repository=message_repository,
        poll_interval_seconds=config.LISTENER_POLL_INTERVAL_SECONDS,
        http_timeout_seconds=config.LISTENER_HTTP_TIMEOUT_SECONDS,
        retry_delay_seconds=config.LISTENER_RETRY_DELAY_SECONDS,
        enabled=config.LISTENER_ENABLE
    )

    await listener.start()

    logger.info("Application started successfully")

    yield

    # Shutdown
    logger.info("Shutting down application...")

    if listener:
        await listener.stop()

    if datasource:
        await datasource.close()

    logger.info("Application shutdown complete")


# Crear la aplicación FastAPI
app = FastAPI(
    title="Messaging System API",
    description="Sistema de mensajería asíncrona - Hackathon COLBA",
    version="1.0.0",
    lifespan=lifespan
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# Dependency injection override
def get_message_repository():
    return message_repository


# Sobrescribir las dependencias en routes
app.dependency_overrides[routes.get_message_repository] = get_message_repository

# Incluir las rutas
app.include_router(routes.router, prefix="/api/v1", tags=["messaging"])


@app.get("/")
async def root():
    """Root endpoint"""
    return {
        "service": "Messaging System",
        "version": "1.0.0",
        "status": "running"
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host=config.API_HOST, port=config.API_PORT)


#
# # Pydantic model for item
# class Item(BaseModel):
#     url: str
#     payload: str
#

# # GET endpoint
# @app.get("/items/")
# def get_item(url: str, payload: str):
#     items = [Item(url=f"url_{i}", payload=f"payload_{i}") for i in range(1, 21)]
#     for item in items:
#         if item.url == url and item.payload == payload:
#             logger.info(f"Returning item: {item}")
#             return item
#     return {"message": "Item not found"}
#
#
# # POST endpoint
# @app.post("/items/")
# def create_item(item: Item):
#     logger.info(f"Creating item: {item}")
#     return {"message": "Item created successfully!", "item": item}