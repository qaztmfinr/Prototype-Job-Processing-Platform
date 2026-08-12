using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using JobProcessingPlatform.Infrastructure.Persistence;
using JobProcessingPlatform.Infrastructure.Repositories;
using JobProcessingPlatform.Infrastructure.Queue;
using JobProcessingPlatform.Infrastructure.Authentication;
using JobProcessingPlatform.Domain.Interfaces;
using JobProcessingPlatform.Application.Services;
using JobProcessingPlatform.API.Middleware;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "JobProcessingPlatform";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "JobProcessingPlatformAPI";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Job Processing Platform API",
        Version = "v1",
        Description = "Distributed job processing platform with Redis queue, EF Core, and JWT authentication",
        Contact = new OpenApiContact
        {
            Name = "Development Team",
            Url = new Uri("https://github.com/yourrepo")
        }
    });

    // JWT Authentication
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme"
    };

    var securityRequirement = new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(securityRequirement);
});

// Database
var dbProvider = builder.Configuration["Database:Provider"] ?? "PostgreSQL";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not configured");

if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<JobProcessingDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<JobProcessingDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Redis
var redisConnection = builder.Configuration["Redis:Connection"] ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton(redis);

// Repositories
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Queue
builder.Services.AddScoped<IJobQueue, RedisJobQueue>();

// Authentication & Security
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService>(sp => new TokenService(jwtSecret, jwtIssuer, jwtAudience, 60));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Processing Platform API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JobProcessingDbContext>();
    try
    {
        if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        // Seed sample data
        await SeedDataAsync(dbContext, scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization");
    }
}

app.Run();

async Task SeedDataAsync(JobProcessingDbContext context, IServiceProvider serviceProvider)
{
    if (await context.Users.AnyAsync())
        return;

    var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
    var adminUser = Domain.Entities.User.Create(
        "admin",
        "admin@example.com",
        passwordService.HashPassword("AdminPassword123!"),
        Domain.Enums.UserRole.Admin);

    var normalUser = Domain.Entities.User.Create(
        "user",
        "user@example.com",
        passwordService.HashPassword("UserPassword123!"),
        Domain.Enums.UserRole.User);

    context.Users.Add(adminUser);
    context.Users.Add(normalUser);
    await context.SaveChangesAsync();
}
