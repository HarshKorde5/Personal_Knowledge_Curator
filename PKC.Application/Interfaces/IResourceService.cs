namespace PKC.Application.Interfaces;

using PKC.Application.DTOs;

public interface IResourceService
{
    Task<Guid> CreateFromUrlAsync(Guid userId, CreateResourceDto dto);

    Task<Guid> CreateNoteAsync(Guid userId, CreateResourceDto dto);
    Task<Guid> CreateFromPdfAsync(Guid userId, string filePath, string? title);
}