namespace PKC.Application.Interfaces;

using PKC.Domain.Entities;

public interface IResourceRepository
{
    Task AddAsync(Resource resource);
    Task<IEnumerable<Resource>> GetByUserIdAsync(Guid userId);
    Task SaveChangesAsync();
}