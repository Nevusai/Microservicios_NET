using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orders.API.Data;
using SharedModels.Contracts;
using SharedModels.Events;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=OrdersDb;Username=postgres;Password=postgres";
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(connectionString));

// HttpClients para comunicación con otros servicios
builder.Services.AddHttpClient("users", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:UsersBaseUrl"] ?? "http://localhost:5002");
});

builder.Services.AddHttpClient("products", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:ProductsBaseUrl"] ?? "http://localhost:5001");
});

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((_, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var user = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var pass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(host, "/", h =>
        {
            h.Username(user);
            h.Password(pass);
        });
    });
});

var app = builder.Build();

// Auto-migrar base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/orders", async (
    CreateOrderRequest req,
    IHttpClientFactory clientFactory,
    IPublishEndpoint publisher,
    OrdersDbContext db) =>
{
    var usersClient = clientFactory.CreateClient("users");
    var productsClient = clientFactory.CreateClient("products");

    // Validar usuario
    var userResp = await usersClient.GetAsync($"/api/users/{req.UserId}");
    if (!userResp.IsSuccessStatusCode) return Results.BadRequest("Usuario no existe");

    // Validar producto
    var product = await productsClient.GetFromJsonAsync<ProductDto>($"/api/products/{req.ProductId}");
    if (product is null) return Results.BadRequest("Producto no existe");
    if (product.Stock < req.Quantity) return Results.BadRequest("Stock insuficiente");

    // Crear orden
    var order = new Order
    {
        UserId = req.UserId,
        ProductId = req.ProductId,
        Quantity = req.Quantity,
        UnitPrice = product.Price,
        Total = product.Price * req.Quantity,
        Status = "Created",
        CreatedAt = DateTime.UtcNow
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    // Publicar evento
    await publisher.Publish(new OrderCreatedEvent(
        order.Id,
        req.UserId,
        req.ProductId,
        req.Quantity,
        product.Price,
        DateTime.UtcNow
    ));

    Console.WriteLine($"[Orders] Orden creada: Id={order.Id}, Total={order.Total}");

    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapGet("/api/orders", async (OrdersDbContext db) =>
{
    var orders = await db.Orders.ToListAsync();
    return Results.Ok(orders);
});

app.MapGet("/api/orders/{id:int}", async (int id, OrdersDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.Run();
