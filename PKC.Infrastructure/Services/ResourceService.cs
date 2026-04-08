namespace PKC.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using PKC.Domain.Entities;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _repo;
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(IResourceRepository repo, IBackgroundTaskQueue queue, ILogger<ResourceService> logger)
    {
        _repo = repo;
        _queue = queue;
        _logger = logger;
    }

    public async Task<Guid> CreateFromUrlAsync(Guid userId, CreateResourceDto dto)
    {
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ResourceType.Url,
            SourceUrl = dto.Url,
            Title = dto.Title,
            Status = ResourceStatus.Pending
        };

        _logger.LogInformation("Saving URL resource for user {UserId}", userId);

        await _repo.AddAsync(resource);
        await _repo.SaveChangesAsync();

        _queue.QueueResource(resource.Id);

        _logger.LogInformation("URL resource saved and queued with ID {ResourceId}", resource.Id);

        return resource.Id;
    }

    public async Task<Guid> CreateNoteAsync(Guid userId, CreateResourceDto dto)
    {
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ResourceType.Note,
            RawContent = dto.Content,
            Title = dto.Title,
            Status = ResourceStatus.Pending
        };

        _logger.LogInformation("Saving note resource for user {UserId}", userId);

        await _repo.AddAsync(resource);
        await _repo.SaveChangesAsync();

        _queue.QueueResource(resource.Id);

        _logger.LogInformation("Note resource saved and queued with ID {ResourceId}", resource.Id);

        return resource.Id;
    }

    public async Task<Guid> CreateFromPdfAsync(Guid userId, string filePath, string? title)
    {
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = ResourceType.Pdf,
            FilePath = filePath,
            Title = title,
            Status = ResourceStatus.Pending
        };

        _logger.LogInformation("Saving PDF resource for user {UserId}, file: {FilePath}", userId, filePath);

        await _repo.AddAsync(resource);
        await _repo.SaveChangesAsync();

        _queue.QueueResource(resource.Id);

        _logger.LogInformation("PDF resource saved and queued with ID {ResourceId}", resource.Id);

        return resource.Id;
    }
}