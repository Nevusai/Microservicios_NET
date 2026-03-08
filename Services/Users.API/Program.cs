using Microsoft.EntityFrameworkCore;
using SharedModels.Contracts;
using Users.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=UsersDb;Username=postgres;Password=postgres";
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Auto-migrar base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/users", async (UsersDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(users.Select(u => new UserDto(u.Id, u.Email, u.Name)));
});

app.MapGet("/api/users/{id:int}", async (int id, UsersDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    return Results.Ok(new UserDto(user.Id, user.Email, user.Name));
});

app.MapPost("/api/users", async (UserDto dto, UsersDbContext db) =>
{
    var user = new User
    {
        Email = dto.Email,
        Name = dto.Name,
        PasswordHash = "hash_placeholder",
        CreatedAt = DateTime.UtcNow
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", new UserDto(user.Id, user.Email, user.Name));
});

app.Run();
