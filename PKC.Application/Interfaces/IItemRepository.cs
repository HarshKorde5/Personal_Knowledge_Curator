namespace PKC.Application.Interfaces;

using PKC.Domain.Entities;

public interface IItemRepository
{
    Task AddAsync(Item item);
    Task SaveChangesAsync();
}