namespace PKC.Application.Interfaces;

using PKC.Application.DTOs;

public interface IItemService
{
    Task<Guid> CreateFromUrlAsync(Guid userId, CreateItemDto dto);

    Task<Guid> CreateNoteAsync(Guid userId, CreateItemDto dto);

    Task<Guid> CreateFromPdfAsync(Guid userId, string filePath, string? title);
    Task<List<ItemListDto>> GetItemsAsync(Guid userId);

    Task<ItemDetailDto?> GetItemAsync(Guid itemId, Guid userId);

    Task<ItemStatusDto?> GetItemStatusAsync(Guid itemId, Guid userId);

    Task<bool> DeleteItemAsync(Guid itemId, Guid userId);
}