using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EcommerceApp.Application.Interfaces.Repositories;
using EcommerceApp.Application.Interfaces.Services;
using EcommerceApp.Application.Services;
using EcommerceApp.Infrastructure.Persistence;
using EcommerceApp.Infrastructure.Repositories;
using EcommerceApp.Infrastructure.Services;

// Build the web application using the provided configuration settings.
var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization options for controllers.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Register services for API exploration and Swagger/OpenAPI documentation.
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication support.
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce API",
        Version = "v1",
        Description = "Ecommerce Application Backend API",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@ecommerce.com"
        }
    });

    // Add JWT authentication to Swagger.
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' followed by a space and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register memory caching services for performance optimization.
builder.Services.AddMemoryCache();

// Configure MySQL database connection with Entity Framework Core.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// Register repository layer services for data access.
builder.Services.AddScoped<IUserRepository, UserRepositories>();
//builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register business logic layer services.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure JWT authentication.
var jwtKey = builder.Configuration["JWT:Secret"] ?? "your-super-secret-key-minimum-32-characters-long-for-jwt-security";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"] ?? "EcommerceApp",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"] ?? "EcommerceAppUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true
    };

    // Configure JWT bearer events for custom token handling.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Try to get token from cookie if not in Authorization header.
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies["refreshToken"];
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configure authorization policies.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User", "Admin"));
    options.AddPolicy("SellerOnly", policy => policy.RequireRole("Seller", "Admin"));
});

// Configure CORS (Cross-Origin Resource Sharing) for frontend applications.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Build the application after all services are registered.
var app = builder.Build();

// Configure the HTTP request pipeline.

// Enable Swagger and Swagger UI only in development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API V1");
        c.RoutePrefix = "swagger";
    });
}

// Enable global exception handling for development.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable CORS middleware.
app.UseCors("AllowFrontend");

// Redirect HTTP requests to HTTPS for security.
app.UseHttpsRedirection();

// Enable static files serving (for images, uploads, etc.).
app.UseStaticFiles();

// Enable routing middleware.
app.UseRouting();

// Enable authentication middleware to validate JWT tokens.
app.UseAuthentication();

// Enable authorization middleware to enforce security policies.
app.UseAuthorization();

// Map incoming requests to their corresponding controller actions.
app.MapControllers();

// Create database and apply pending migrations on startup.
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    if (dbContext.Database.GetPendingMigrations().Any())
//    {
//        dbContext.Database.Migrate();
//    }
//}

// Start the web application and begin listening for requests.
app.Run();