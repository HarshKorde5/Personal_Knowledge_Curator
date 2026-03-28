namespace PKC.Application.Interfaces;

using PKC.Application.DTOs;
using PKC.Domain.Entities;

public interface IItemRepository
{
    Task AddAsync(Item item);
    Task SaveChangesAsync();
    Task<List<ItemListDto>> GetAllByUserAsync(Guid userId);
    Task<ItemDetailDto?> GetByIdAsync(Guid itemId, Guid userId);
    Task<ItemStatusDto?> GetStatusAsync(Guid itemId, Guid userId);
    Task<bool> DeleteAsync(Guid itemId, Guid userId);
}