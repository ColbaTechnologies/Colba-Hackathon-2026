"""
Configuration module
Carga configuración desde variables de entorno
"""
import os
from dotenv import load_dotenv

load_dotenv()


class Config:
    """Configuración de la aplicación"""

    # Database
    DB_USER = os.getenv("DB_USER", "messaging_user")
    DB_PASSWORD = os.getenv("DB_PASSWORD", "messaging_pass")
    DB_HOST = os.getenv("DB_HOST", "localhost")
    DB_PORT = os.getenv("DB_PORT", "5432")
    DB_NAME = os.getenv("DB_NAME", "messaging_db")

    # Listener - Worker
    LISTENER_ENABLE = os.getenv("LISTENER_ENABLE", "true").lower() == "true"
    LISTENER_POLL_INTERVAL_SECONDS = float(os.getenv("LISTENER_POLL_INTERVAL_SECONDS", "2"))
    LISTENER_HTTP_TIMEOUT_SECONDS = float(os.getenv("LISTENER_HTTP_TIMEOUT_SECONDS", "10"))
    LISTENER_RETRY_DELAY_SECONDS = float(os.getenv("LISTENER_RETRY_DELAY_SECONDS", "30"))

    @property
    def database_url(self) -> str:
        return f"postgresql://{self.DB_USER}:{self.DB_PASSWORD}@{self.DB_HOST}:{self.DB_PORT}/{self.DB_NAME}"


    # API
    API_HOST = os.getenv("API_HOST", "0.0.0.0")
    API_PORT = int(os.getenv("API_PORT", "8000"))

    # Connection Pool
    DB_POOL_MIN_SIZE = int(os.getenv("DB_POOL_MIN_SIZE", "10"))
    DB_POOL_MAX_SIZE = int(os.getenv("DB_POOL_MAX_SIZE", "20"))


config = Config()