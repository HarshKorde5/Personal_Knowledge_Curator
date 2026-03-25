namespace PKC.Infrastructure.Repositories;

using PKC.Application.Interfaces;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Item item)
    {
        await _context.Items.AddAsync(item);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}