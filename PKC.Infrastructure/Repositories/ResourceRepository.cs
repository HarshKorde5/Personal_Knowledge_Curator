namespace PKC.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using PKC.Application.Interfaces;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _context;

    public ResourceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Resource resource)
    {
        await _context.Resources.AddAsync(resource);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Resource>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Resources
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt) // Good practice to show newest first
            .ToListAsync();
    }
}