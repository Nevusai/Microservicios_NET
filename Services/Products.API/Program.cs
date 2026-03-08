using MassTransit;
using Microsoft.EntityFrameworkCore;
using Products.API.Data;
using SharedModels.Contracts;
using SharedModels.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=ProductsDb;Username=postgres;Password=postgres";
builder.Services.AddDbContext<ProductsDbContext>(options =>
    options.UseNpgsql(connectionString));

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var user = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(pass);
        });

        cfg.ReceiveEndpoint("products-order-created", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

var app = builder.Build();

// Auto-migrar base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/products", async (ProductsDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Stock)));
});

app.MapGet("/api/products/{id:int}", async (int id, ProductsDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    return Results.Ok(new ProductDto(product.Id, product.Name, product.Price, product.Stock));
});

app.MapPost("/api/products", async (ProductDto dto, ProductsDbContext db) =>
{
    var product = new Product
    {
        Name = dto.Name,
        Price = dto.Price,
        Stock = dto.Stock,
        CreatedAt = DateTime.UtcNow
    };
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", new ProductDto(product.Id, product.Name, product.Price, product.Stock));
});

app.Run();

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ProductsDbContext _db;

    public OrderCreatedConsumer(ProductsDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var evt = context.Message;
        Console.WriteLine($"[Products] OrderCreated: OrderId={evt.OrderId}, ProductId={evt.ProductId}, Qty={evt.Quantity}");

        // Actualizar stock
        var product = await _db.Products.FindAsync(evt.ProductId);
        if (product != null)
        {
            product.Stock -= evt.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            Console.WriteLine($"[Products] Stock actualizado: ProductId={product.Id}, NuevoStock={product.Stock}");
        }
    }
}
