#!/bin/bash

# Script de comandos comunes para gestión del proyecto

case "$1" in
    build)
        echo "🔨 Compilando solución..."
        dotnet build MicroserviciosApp.slnx
        ;;
    
    clean)
        echo "🧹 Limpiando solución..."
        dotnet clean MicroserviciosApp.slnx
        rm -rf Services/*/bin Services/*/obj Shared/*/bin Shared/*/obj
        ;;
    
    docker-build)
        echo "🐳 Construyendo imágenes Docker..."
        docker compose build
        ;;
    
    docker-up)
        echo "🚀 Levantando servicios..."
        docker compose up -d
        echo ""
        echo "✓ Servicios iniciados"
        echo "  Gateway: http://localhost:8000"
        echo "  RabbitMQ: http://localhost:15672 (guest/guest)"
        echo "  PostgreSQL: localhost:5432 (postgres/postgres)"
        ;;
    
    docker-down)
        echo "🛑 Deteniendo servicios..."
        docker compose down
        ;;
    
    docker-logs)
        if [ -z "$2" ]; then
            docker compose logs -f
        else
            docker compose logs -f "$2"
        fi
        ;;
    
    docker-clean)
        echo "🗑️  Limpiando Docker (contenedores, volúmenes, redes)..."
        docker compose down -v
        docker system prune -f
        ;;
    
    migrations-create)
        echo "📝 Creando migraciones..."
        ./manage-migrations.sh
        ;;
    
    test)
        echo "🧪 Ejecutando pruebas..."
        ./test-api.sh
        ;;
    
    restart)
        echo "♻️  Reiniciando servicios..."
        docker compose down
        docker compose up -d
        ;;
    
    status)
        echo "📊 Estado de los servicios:"
        docker compose ps
        ;;
    
    help|*)
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo "  Microservicios .NET - Comandos Rápidos"
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""
        echo "Uso: ./run.sh [comando]"
        echo ""
        echo "Compilación:"
        echo "  build              Compilar solución .NET"
        echo "  clean              Limpiar archivos compilados"
        echo ""
        echo "Docker:"
        echo "  docker-build       Construir imágenes Docker"
        echo "  docker-up          Levantar todos los servicios"
        echo "  docker-down        Detener todos los servicios"
        echo "  docker-logs [svc]  Ver logs (opcional: servicio específico)"
        echo "  docker-clean       Limpiar Docker (contenedores + volúmenes)"
        echo "  restart            Reiniciar servicios"
        echo "  status             Ver estado de servicios"
        echo ""
        echo "Base de Datos:"
        echo "  migrations-create  Crear/gestionar migraciones EF"
        echo ""
        echo "Testing:"
        echo "  test               Ejecutar pruebas de API"
        echo ""
        echo "Ejemplos:"
        echo "  ./run.sh docker-up"
        echo "  ./run.sh docker-logs products-api"
        echo "  ./run.sh test"
        echo ""
        ;;
esac
