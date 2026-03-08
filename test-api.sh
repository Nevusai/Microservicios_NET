#!/bin/bash

# Script de pruebas rápidas de la API

BASE_URL="http://localhost:8000"

echo "========================================"
echo "  Pruebas de Microservicios"
echo "========================================"
echo ""

# Colores
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Función para hacer requests
test_endpoint() {
    local method=$1
    local endpoint=$2
    local data=$3
    local description=$4
    
    echo -e "${BLUE}${description}${NC}"
    echo "→ ${method} ${BASE_URL}${endpoint}"
    
    if [ -z "$data" ]; then
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X ${method} "${BASE_URL}${endpoint}")
    else
        response=$(curl -s -w "\nHTTP_CODE:%{http_code}" -X ${method} "${BASE_URL}${endpoint}" \
            -H "Content-Type: application/json" \
            -d "${data}")
    fi
    
    http_code=$(echo "$response" | grep "HTTP_CODE:" | cut -d':' -f2)
    body=$(echo "$response" | sed '/HTTP_CODE:/d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓ ${http_code}${NC}"
    else
        echo -e "${RED}✗ ${http_code}${NC}"
    fi
    
    echo "$body" | jq '.' 2>/dev/null || echo "$body"
    echo ""
}

# Verificar que los servicios estén corriendo
echo -e "${YELLOW}Verificando que los servicios estén corriendo...${NC}"
if ! curl -s http://localhost:8000 > /dev/null 2>&1; then
    echo -e "${RED}Error: Gateway no está corriendo en el puerto 8000${NC}"
    echo "Ejecuta: docker compose up -d"
    exit 1
fi
echo -e "${GREEN}✓ Gateway está corriendo${NC}"
echo ""

# Tests
echo -e "${YELLOW}=== PRODUCTS API ===${NC}"
test_endpoint "GET" "/products" "" "1. Listar todos los productos"
test_endpoint "GET" "/products/1" "" "2. Obtener producto por ID"
test_endpoint "POST" "/products" '{"name":"Teclado RGB","price":80,"stock":25}' "3. Crear nuevo producto"

echo -e "${YELLOW}=== USERS API ===${NC}"
test_endpoint "GET" "/users" "" "4. Listar todos los usuarios"
test_endpoint "GET" "/users/1" "" "5. Obtener usuario por ID"
test_endpoint "POST" "/users" '{"email":"carlos@example.com","name":"Carlos Lopez"}' "6. Crear nuevo usuario"

echo -e "${YELLOW}=== ORDERS API ===${NC}"
test_endpoint "GET" "/orders" "" "7. Listar todas las órdenes"
test_endpoint "POST" "/orders" '{"userId":1,"productId":1,"quantity":2}' "8. Crear nueva orden (HTTP + RabbitMQ)"

echo ""
echo -e "${YELLOW}Esperando 2 segundos para procesamiento asíncrono...${NC}"
sleep 2

echo ""
echo -e "${GREEN}=== VERIFICACIÓN ===${NC}"
echo "1. Revisa los logs de Products.API para ver el evento consumido:"
echo "   docker compose logs products-api | grep OrderCreated"
echo ""
echo "2. Verifica el stock actualizado:"
test_endpoint "GET" "/products/1" "" "   Stock del producto 1 (debería haber disminuido)"

echo ""
echo -e "${GREEN}¡Pruebas completadas!${NC}"
echo ""
echo "Para ver la UI de RabbitMQ:"
echo "→ http://localhost:15672 (guest/guest)"
echo ""
echo "Para ver Swagger de cada API:"
echo "→ http://localhost:5001/swagger (Products)"
echo "→ http://localhost:5002/swagger (Users)"
echo "→ http://localhost:5003/swagger (Orders)"
