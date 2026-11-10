using HVC_Comics.Models;

namespace HVC_Comics.Repositories;

public interface IComicRepository
{
    Task<PaginationResult<Comic>> GetPagedAsync(int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default);
    Task<Comic?> GetRandomAsync(CancellationToken cancellationToken = default);
    Task<Comic?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Comic?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Comic>> GetEditionsAsync(string baseName, int volume, CancellationToken cancellationToken = default);
    Task<List<Comic>> GetVolumeStartsAsync(string baseName, CancellationToken cancellationToken = default);
}
