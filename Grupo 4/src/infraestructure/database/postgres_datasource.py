import asyncpg
from contextlib import asynccontextmanager
from typing import Optional
import logging

logger = logging.getLogger(__name__)


class PostgresDataSource:
    """Fuente de datos PostgreSQL con pool de conexiones"""

    def __init__(self, connection_string: str, min_size: int = 10, max_size: int = 20):
        self.connection_string = connection_string
        self.min_size = min_size
        self.max_size = max_size
        self.pool: Optional[asyncpg.Pool] = None

    async def initialize(self):
        """Inicializa el pool de conexiones"""
        try:
            self.pool = await asyncpg.create_pool(
                self.connection_string,
                min_size=self.min_size,
                max_size=self.max_size,
                command_timeout=60
            )
            logger.info("PostgreSQL connection pool initialized successfully")
        except Exception as e:
            logger.error(f"Failed to initialize PostgreSQL pool: {e}")
            raise

    async def close(self):
        """Cierra el pool de conexiones"""
        if self.pool:
            await self.pool.close()
            logger.info("PostgreSQL connection pool closed")

    @asynccontextmanager
    async def get_connection(self):
        """Context manager para obtener una conexión del pool"""
        if not self.pool:
            raise RuntimeError("Database pool not initialized. Call initialize() first.")

        async with self.pool.acquire() as connection:
            yield connection

    async def execute(self, query: str, *args):
        """Ejecuta una query sin retorno"""
        async with self.get_connection() as conn:
            return await conn.execute(query, *args)

    async def fetch(self, query: str, *args):
        """Ejecuta una query y retorna múltiples resultados"""
        async with self.get_connection() as conn:
            return await conn.fetch(query, *args)

    async def fetchrow(self, query: str, *args):
        """Ejecuta una query y retorna un solo resultado"""
        async with self.get_connection() as conn:
            return await conn.fetchrow(query, *args)