namespace PKC.Infrastructure.Services;

using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using PKC.Domain.Entities;

public class ItemService : IItemService
{
    private readonly IItemRepository _repo;
    private readonly IBackgroundTaskQueue _queue;

    public ItemService(IItemRepository repo, IBackgroundTaskQueue queue)
    {
        _repo = repo;
        _queue = queue;
    }
    public async Task<Guid> CreateUrlAsync(Guid userId, CreateItemDto dto)
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

        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();
        _queue.QueueItem(item.Id);
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

        await _repo.AddAsync(item);
        await _repo.SaveChangesAsync();

        return item.Id;
    }
}