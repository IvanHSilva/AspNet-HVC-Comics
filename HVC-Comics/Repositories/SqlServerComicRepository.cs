public class SqlServerComicRepository(SqlServerConnectionFactory factory)
    : IComicRepository
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public PaginationResult<Comic> GetPaged(
        int page = 1,
        int pageSize = 50)
    {
        // implementação SQL Server
    }
}

