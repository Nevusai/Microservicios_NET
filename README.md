# Microservicios .NET Core con PostgreSQL y RabbitMQ

Arquitectura de microservicios con .NET 10, PostgreSQL, RabbitMQ y Ocelot Gateway.

## 🏗️ Arquitectura

```
┌─────────────┐
│   Cliente   │
└──────┬──────┘
       │ HTTP
       ▼
┌─────────────────────┐
│  Gateway.API:8000   │  (Ocelot)
└──────┬──────────────┘
       │
   ────┼────────────────────
   │   │              │
   ▼   ▼              ▼
┌────────┐  ┌────────┐  ┌────────┐
│Products│  │ Users  │  │ Orders │
│  :5001 │  │ :5002  │  │ :5003  │
└────┬───┘  └───┬────┘  └───┬────┘
     │          │            │
     └──────────┴────────────┘
           │         │
     ┌─────▼─────┐  │
     │PostgreSQL │  │
     │   :5432   │  │
     └───────────┘  │
           │        │
     ┌─────▼────────▼─┐
     │   RabbitMQ     │
     │   :5672/15672  │
     └────────────────┘
```

## 📋 Servicios

| Servicio | Puerto | Base de Datos | Descripción |
|----------|--------|---------------|-------------|
| **Products.API** | 5001 | ProductsDb | CRUD de productos + Consumer RabbitMQ |
| **Users.API** | 5002 | UsersDb | CRUD de usuarios |
| **Orders.API** | 5003 | OrdersDb | Creación de órdenes + Publisher RabbitMQ |
| **Gateway.API** | 8000 | - | API Gateway (Ocelot) |
| **PostgreSQL** | 5432 | - | Base de datos |
| **RabbitMQ** | 5672/15672 | - | Message Broker |

## 🚀 Inicio Rápido

### 1. Prerequisitos
```bash
# Verificar dotnet
dotnet --version  # Debe ser 10.0+

# Verificar Docker
docker --version
docker-compose --version
```

### 2. Crear Migraciones (Desarrollo Local)
```bash
# Products
cd Services/Products.API
dotnet ef migrations add InitialCreate
dotnet ef database update

# Users
cd ../Users.API
dotnet ef migrations add InitialCreate
dotnet ef database update

# Orders
cd ../Orders.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Ejecutar con Docker
```bash
# Construir imágenes
docker compose build

# Levantar todos los servicios
docker compose up -d

# Ver logs
docker compose logs -f

# Ver estado
docker compose ps
```

### 4. Verificar Servicios

**RabbitMQ Management:**
- URL: http://localhost:15672
- Usuario: `guest`
- Contraseña: `guest`

**Swagger (APIs individuales):**
- Products: http://localhost:5001/swagger
- Users: http://localhost:5002/swagger
- Orders: http://localhost:5003/swagger

**Gateway (punto de entrada único):**
- http://localhost:8000
  - GET /products
  - GET /products/{id}
  - POST /products
  - GET /users
  - GET /users/{id}
  - POST /users
  - GET /orders
  - GET /orders/{id}
  - POST /orders

## 📡 Probar la API

### Listar productos (vía Gateway)
```bash
curl http://localhost:8000/products
```

### Obtener un producto
```bash
curl http://localhost:8000/products/1
```

### Crear una orden (HTTP síncrono + evento RabbitMQ asíncrono)
```bash
curl -X POST http://localhost:8000/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "productId": 1,
    "quantity": 2
  }'
```

**Lo que sucede:**
1. Orders.API valida el usuario llamando a Users.API (HTTP)
2. Orders.API valida el producto llamando a Products.API (HTTP)
3. Orders.API guarda la orden en PostgreSQL
4. Orders.API publica el evento `OrderCreatedEvent` en RabbitMQ
5. Products.API consume el evento y actualiza el stock del producto

### Ver logs del consumer
```bash
docker compose logs -f products-api
```

Deberías ver:
```
[Products] OrderCreated: OrderId=1, ProductId=1, Qty=2
[Products] Stock actualizado: ProductId=1, NuevoStock=8
```

## 🛠️ Desarrollo Local (sin Docker)

### Terminal 1: PostgreSQL + RabbitMQ
```bash
docker compose up postgres rabbitmq
```

### Terminal 2-5: Ejecutar servicios
```bash
# Terminal 2
cd Services/Products.API && dotnet run

# Terminal 3
cd Services/Users.API && dotnet run

# Terminal 4
cd Services/Orders.API && dotnet run

# Terminal 5
cd Services/Gateway.API && dotnet run
```

## 🔧 Comandos Útiles

### Docker
```bash
# Detener todos los servicios
docker compose down

# Eliminar volúmenes (limpia BD)
docker compose down -v

# Reconstruir imágenes
docker compose build --no-cache

# Ver logs de un servicio específico
docker compose logs -f products-api

# Reiniciar un servicio
docker compose restart products-api

# Ejecutar comando en contenedor
docker compose exec postgres psql -U postgres
```

### Entity Framework
```bash
# Crear migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir última migración
dotnet ef migrations remove

# Ver SQL de migración
dotnet ef migrations script
```

### Compilación
```bash
# Compilar solución
dotnet build MicroserviciosApp.slnx

# Limpiar solución
dotnet clean MicroserviciosApp.slnx

# Restaurar paquetes
dotnet restore MicroserviciosApp.slnx

# Ejecutar tests (si existen)
dotnet test
```

## 📂 Estructura del Proyecto

```
microservicios/
├── Services/
│   ├── Products.API/
│   │   ├── Data/
│   │   │   ├── Product.cs
│   │   │   └── ProductsDbContext.cs
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── Users.API/
│   │   ├── Data/
│   │   │   ├── User.cs
│   │   │   └── UsersDbContext.cs
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── Orders.API/
│   │   ├── Data/
│   │   │   ├── Order.cs
│   │   │   └── OrdersDbContext.cs
│   │   ├── Dockerfile
│   │   └── Program.cs
│   └── Gateway.API/
│       ├── Dockerfile
│       ├── ocelot.json
│       └── Program.cs
├── Shared/
│   └── SharedModels/
│       ├── Contracts/
│       │   ├── ProductDto.cs
│       │   ├── UserDto.cs
│       │   └── CreateOrderRequest.cs
│       └── Events/
│           └── OrderCreatedEvent.cs
├── docker-compose.yml
├── .dockerignore
├── MicroserviciosApp.slnx
└── README.md
```

## 🔐 Variables de Entorno

Las variables se configuran en `docker-compose.yml`:

```yaml
# PostgreSQL
ConnectionStrings__DefaultConnection=Host=postgres;Database=ProductsDb;Username=postgres;Password=postgres

# RabbitMQ
RabbitMQ__Host=rabbitmq
RabbitMQ__Username=guest
RabbitMQ__Password=guest

# URLs de otros servicios
Services__UsersBaseUrl=http://users-api
Services__ProductsBaseUrl=http://products-api
```

## 🐛 Troubleshooting

### Error: "No se puede conectar a PostgreSQL"
- Verifica que el contenedor esté corriendo: `docker compose ps`
- Espera a que esté healthy: `docker compose logs postgres`

### Error: "No se puede conectar a RabbitMQ"
- Verifica: `docker compose logs rabbitmq`
- Accede al Management UI: http://localhost:15672

### Error: "Gateway no puede encontrar servicios"
- Verifica que los servicios estén corriendo: `docker compose ps`
- Revisa la configuración en `ocelot.json`

### Limpiar TODO y empezar de cero
```bash
docker compose down -v
docker system prune -a
rm -rf Services/*/Migrations
dotnet clean
dotnet build
```

## 📚 Tecnologías Utilizadas

- **.NET 10** - Framework
- **PostgreSQL 15** - Base de datos
- **Entity Framework Core** - ORM
- **MassTransit** - Abstracción de mensajería
- **RabbitMQ** - Message broker
- **Ocelot** - API Gateway
- **Swashbuckle (Swagger)** - Documentación API
- **Docker & Docker Compose** - Containerización

## 🎯 Próximos Pasos

- [ ] Agregar autenticación JWT
- [ ] Implementar Circuit Breaker (Polly)
- [ ] Agregar logs centralizados (Seq, ELK)
- [ ] Implementar Health Checks
- [ ] Agregar tests unitarios e integración
- [ ] Implementar API versioning
- [ ] Agregar Redis para caching
- [ ] Configurar CI/CD

## 📄 Licencia

MIT
