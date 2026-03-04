.PHONY: help setup start run stop clean test db-up db-down logs

help:
	@echo "Sistema de Mensajería Asíncrona - Comandos disponibles:"
	@echo ""
	@echo "  make setup      - Configura el proyecto por primera vez"
	@echo "  make start      - Inicia PostgreSQL con Docker"
	@echo "  make run        - Inicia la aplicación"
	@echo "  make stop       - Detiene PostgreSQL"
	@echo "  make clean      - Limpia todo (incluyendo datos)"
	@echo "  make logs       - Muestra logs de PostgreSQL"
	@echo "  make db-up      - Inicia solo la base de datos"
	@echo "  make db-down    - Detiene la base de datos"
	@echo ""

setup:
	@echo "📦 Configurando proyecto..."
	@cp -n .env.example .env || true
	@chmod +x start.sh
	@pip3 install -r requirements.txt
	@echo "✅ Configuración completa"

start: db-up
	@echo "✅ Base de datos iniciada"

run:
	@echo "🚀 Iniciando API..."
	@cd src && python3 main.py

db-up:
	@echo "🐘 Iniciando PostgreSQL..."
	@docker-compose up -d
	@echo "⏳ Esperando PostgreSQL..."
	@sleep 5
	@echo "✅ PostgreSQL listo"

db-down:
	@echo "🛑 Deteniendo PostgreSQL..."
	@docker-compose down

stop: db-down

clean:
	@echo "🧹 Limpiando proyecto..."
	@docker-compose down -v
	@find . -type d -name __pycache__ -exec rm -rf {} + 2>/dev/null || true
	@find . -type f -name "*.pyc" -delete 2>/dev/null || true
	@echo "✅ Limpieza completa"

logs:
	@docker-compose logs -f postgres

test:
	@echo "🧪 Ejecutando tests..."
	@cd tests && python3 -m pytest

.DEFAULT_GOAL := help