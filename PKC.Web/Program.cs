using Microsoft.EntityFrameworkCore;
using PKC.Infrastructure.Data;
using PKC.Application.Interfaces;
using PKC.Infrastructure.Repositories;
using PKC.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//  1. Database Configuration 
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions => { 
        npgsqlOptions.UseVector();
        npgsqlOptions.CommandTimeout(300);
    }));

// 2. Authentication & JWT 
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// 3. Dependency Injection (Services & Repositories)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Core Auth & Data
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

// Background Processing
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<ItemProcessingService>();
builder.Services.AddHostedService<ProcessingWorker>();

// Content & AI Services (Cleaned up duplicates)
builder.Services.AddHttpClient<ContentExtractor>();
builder.Services.AddScoped<ContentExtractor>();

builder.Services.AddScoped<ChunkingService>();

builder.Services.AddHttpClient<EmbeddingService>();
builder.Services.AddScoped<EmbeddingService>();

builder.Services.AddHttpClient<AiService>(client => client.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddScoped<AiService>();

builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<ResurfacingService>();

var app = builder.Build();

// 4. Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();