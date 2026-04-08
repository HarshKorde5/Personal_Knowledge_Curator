namespace PKC.Infrastructure.Repositories;

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
}