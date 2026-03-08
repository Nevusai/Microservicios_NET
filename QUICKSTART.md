# 🚀 Inicio Rápido

## ⚡ 3 pasos para ejecutar

```bash
# 1. Compilar
./run.sh build

# 2. Levantar contenedores
./run.sh docker-build
./run.sh docker-up

# 3. Probar
./run.sh test
```

## ✅ Verificación

**Gateway API:**
```bash
curl http://localhost:8000/products
```

**RabbitMQ Management:**
- URL: http://localhost:15672
- User: `guest` / Pass: `guest`

**Swagger:**
- Products: http://localhost:5001/swagger
- Users: http://localhost:5002/swagger  
- Orders: http://localhost:5003/swagger

## 📝 Crear una orden

```bash
curl -X POST http://localhost:8000/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "productId": 1,
    "quantity": 2
  }'
```

Esto:
1. ✅ Valida usuario en Users.API (HTTP)
2. ✅ Valida producto en Products.API (HTTP)
3. ✅ Guarda orden en PostgreSQL
4. ✅ Publica evento en RabbitMQ
5. ✅ Products.API consume evento y actualiza stock

## 🔍 Ver el evento procesado

```bash
./run.sh docker-logs products-api
```

Busca:
```
[Products] OrderCreated: OrderId=1, ProductId=1, Qty=2
[Products] Stock actualizado: ProductId=1, NuevoStock=8
```

## 🛑 Detener todo

```bash
./run.sh docker-down
```

## 📚 Más información

Ver [README.md](README.md) completo.
