public class MySqlComicRepository : IComicRepository
{
    public PaginationResult<Comic> GetPaged(
        int page = 1,
        int pageSize = 50)
    {
        // implementação MySQL
    }
}