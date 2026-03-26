using Microsoft.EntityFrameworkCore;
using PKC.Infrastructure.Data;
using PKC.Application.Interfaces;
using PKC.Infrastructure.Repositories;
using PKC.Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), x => { 
    x.UseVector();
    x.CommandTimeout(300);
}));

builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<ItemProcessingService>();
builder.Services.AddHostedService<ProcessingWorker>();

builder.Services.AddHttpClient<ContentExtractor>();
builder.Services.AddScoped<ContentExtractor>();

builder.Services.AddScoped<ChunkingService>();
builder.Services.AddScoped<EmbeddingService>();

builder.Services.AddScoped<SearchService>();
builder.Services.AddHttpClient<AiService>(client => client.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddScoped<AiService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddHttpClient<EmbeddingService>();
builder.Services.AddScoped<EmbeddingService>();

builder.Services.AddScoped<ConnectionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();