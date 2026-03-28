namespace PKC.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using PKC.Domain.Entities;

public class ItemService : IItemService
{
    private readonly IItemRepository _repo;
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<ItemService> _logger;

    public ItemService(IItemRepository repo, IBackgroundTaskQueue queue, ILogger<ItemService> logger)
    {
        _repo = repo;
        _queue = queue;
        _logger = logger;
    }

    public async Task<Guid> CreateFromUrlAsync(Guid userId, CreateItemDto dto)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ItemType.Url,
            SourceUrl = dto.Url,
            Title = dto.Title,
            Status = ItemStatus.Pending
        };

        _logger.LogInformation("Saving URL item for user {UserId}", userId);

        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();

        _queue.QueueItem(item.Id);

        _logger.LogInformation("URL item saved and queued with ID {ItemId}", item.Id);

        return item.Id;
    }

    public async Task<Guid> CreateNoteAsync(Guid userId, CreateItemDto dto)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ItemType.Note,
            RawContent = dto.Content,
            Title = dto.Title,
            Status = ItemStatus.Pending
        };

        _logger.LogInformation("Saving note item for user {UserId}", userId);

        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();

        _queue.QueueItem(item.Id);

        _logger.LogInformation("Note item saved and queued with ID {ItemId}", item.Id);

        return item.Id;
    }

    public async Task<Guid> CreateFromPdfAsync(Guid userId, string filePath, string? title)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ItemType.Pdf,
            FilePath = filePath,
            Title = title,
            Status = ItemStatus.Pending
        };

        _logger.LogInformation("Saving PDF item for user {UserId}, file: {FilePath}", userId, filePath);

        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();

        _queue.QueueItem(item.Id);

        _logger.LogInformation("PDF item saved and queued with ID {ItemId}", item.Id);

        return item.Id;
    }
}