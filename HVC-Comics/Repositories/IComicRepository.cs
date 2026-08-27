using HVC_Comics.Models;

namespace HVC_Comics.Repositories;

public interface IComicRepository
{
    PaginationResult<Comic> GetPaged(int page = 1, int pageSize = 50);
}