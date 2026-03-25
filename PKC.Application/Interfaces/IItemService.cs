namespace PKC.Application.Interfaces;

using PKC.Application.DTOs;

public interface IItemService
{
    Task<Guid> CreateUrlAsync(Guid userId, CreateItemDto dto);
    Task<Guid> CreateNoteAsync(Guid userId, CreateItemDto dto);
}