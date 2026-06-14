using Microsoft.EntityFrameworkCore;
using EcommerceApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add basic controller support
builder.Services.AddControllers();

// ✅ MODELS & DATABASE CONFIGURATION ONLY
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "server=localhost;port=3306;database=ecommerce_db;user=root;";

    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)));
});

var app = builder.Build();

// Basic HTTP pipeline routing
app.UseRouting();

// Map your API endpoints
app.MapControllers();

app.Run();