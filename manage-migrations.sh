#!/bin/bash

# Script para gestionar migraciones de Entity Framework

echo "==================================="
echo "  Gestión de Migraciones EF Core"
echo "==================================="

# Colores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función para crear migraciones
create_migrations() {
    echo -e "${YELLOW}Creando migraciones...${NC}"
    
    echo -e "${GREEN}Products.API${NC}"
    cd Services/Products.API
    dotnet ef migrations add InitialCreate
    cd ../..
    
    echo -e "${GREEN}Users.API${NC}"
    cd Services/Users.API
    dotnet ef migrations add InitialCreate
    cd ../..
    
    echo -e "${GREEN}Orders.API${NC}"
    cd Services/Orders.API
    dotnet ef migrations add InitialCreate
    cd ../..
    
    echo -e "${GREEN}✓ Migraciones creadas${NC}"
}

# Función para aplicar migraciones (desarrollo local)
apply_migrations() {
    echo -e "${YELLOW}Aplicando migraciones...${NC}"
    
    echo -e "${GREEN}Products.API${NC}"
    cd Services/Products.API
    dotnet ef database update
    cd ../..
    
    echo -e "${GREEN}Users.API${NC}"
    cd Services/Users.API
    dotnet ef database update
    cd ../..
    
    echo -e "${GREEN}Orders.API${NC}"
    cd Services/Orders.API
    dotnet ef database update
    cd ../..
    
    echo -e "${GREEN}✓ Migraciones aplicadas${NC}"
}

# Función para eliminar migraciones
remove_migrations() {
    echo -e "${YELLOW}Eliminando carpetas Migrations...${NC}"
    
    rm -rf Services/Products.API/Migrations
    rm -rf Services/Users.API/Migrations
    rm -rf Services/Orders.API/Migrations
    
    echo -e "${GREEN}✓ Migraciones eliminadas${NC}"
}

# Función para generar script SQL
generate_sql() {
    echo -e "${YELLOW}Generando scripts SQL...${NC}"
    
    mkdir -p sql-scripts
    
    cd Services/Products.API
    dotnet ef migrations script > ../../sql-scripts/products-migration.sql
    cd ../..
    
    cd Services/Users.API
    dotnet ef migrations script > ../../sql-scripts/users-migration.sql
    cd ../..
    
    cd Services/Orders.API
    dotnet ef migrations script > ../../sql-scripts/orders-migration.sql
    cd ../..
    
    echo -e "${GREEN}✓ Scripts SQL generados en ./sql-scripts/${NC}"
}

# Menú principal
echo ""
echo "Selecciona una opción:"
echo "1) Crear migraciones"
echo "2) Aplicar migraciones (dev local)"
echo "3) Generar scripts SQL"
echo "4) Eliminar migraciones"
echo "5) Limpiar y recrear todo"
echo "0) Salir"
echo ""
read -p "Opción: " option

case $option in
    1)
        create_migrations
        ;;
    2)
        apply_migrations
        ;;
    3)
        generate_sql
        ;;
    4)
        remove_migrations
        ;;
    5)
        remove_migrations
        create_migrations
        apply_migrations
        ;;
    0)
        echo "Saliendo..."
        exit 0
        ;;
    *)
        echo "Opción inválida"
        exit 1
        ;;
esac

echo ""
echo -e "${GREEN}¡Listo!${NC}"
