using Microsoft.EntityFrameworkCore;
using PKC.Infrastructure.Data;
using PKC.Application.Interfaces;
using PKC.Infrastructure.Repositories;
using PKC.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
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

// 3. Dependency Injection

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

// FIX: AddHttpClient<T> already registers T as a scoped service with the managed HttpClient.
// The previous duplicate AddScoped<T> calls were shadowing the typed HttpClient registration,
// meaning services received a plain HttpClient instead of the configured one.
// Removed all duplicate AddScoped calls — AddHttpClient<T> is the only registration needed.

builder.Services.AddHttpClient<ContentExtractor>(client =>
{
    // FIX: Added 30s timeout — previously no timeout meant a slow/hanging URL could
    // block the background worker thread indefinitely, stalling the entire queue.
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<EmbeddingService>();

builder.Services.AddHttpClient<AiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddScoped<ChunkingService>();
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

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();