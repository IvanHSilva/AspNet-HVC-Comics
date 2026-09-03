using HVC_Comics.Models;

namespace HVC_Comics.Repositories;

public class MySqlComicRepository : IComicRepository
{
    public PaginationResult<Comic> GetPaged(
        int page = 1,
        int pageSize = 50)
    {
        throw new NotImplementedException(
            "O repository MySQL ainda não foi implementado.");
    }
}
