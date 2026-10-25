using HVC_Comics.DataFactories;
using HVC_Comics.Models;
using HVC_Comics.Views.Comic;

using Microsoft.Data.SqlClient;
using System.Data;

namespace HVC_Comics.Repositories;

public class SqlServerComicRepository(
SqlServerConnectionFactory factory) : IComicRepository
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public async Task<PaginationResult<Comic>> GetPagedAsync(int page = 1,
        int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = new PaginationResult<Comic>
        { CurrentPage = page, PageSize = pageSize, DataSource = "SQL Server" };

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using (var countCommand = new SqlCommand(
            "SELECT COUNT(*) FROM Revistas", connection))
        {
            result.TotalRecords = Convert.ToInt32(await countCommand.ExecuteScalarAsync(
                cancellationToken));
        }

        var totalPages = result.TotalRecords == 0 ? 1 : (int)Math.Ceiling(
            (double)result.TotalRecords / pageSize);
        page = Math.Min(page, totalPages);
        result.CurrentPage = page;
        var offset = (page - 1) * pageSize;

        const string sql = """
        SELECT Codigo, RevistaBR, EdicaoBR, EditoraBR, EditoraEUA, NomeMesBR,AnoRevBR, Preco
        FROM Revistas ORDER BY Codigo 
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Offset", SqlDbType.Int).Value = offset;
        command.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            result.Items.Add(MapComic(reader));

        return result;
    }

    public async Task<Comic?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT Codigo, RevistaBR, EdicaoBR, EditoraBR, EditoraEUA, NomeMesBR,AnoRevBR, Preco
        FROM Revistas WHERE Codigo = @Id;
        """;

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return MapComic(reader);
    }

    public async Task<Comic?> GetRandomAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT TOP 1 Codigo, RevistaBR, EdicaoBR, EditoraBR, EditoraEUA, NomeMesBR,AnoRevBR, Preco
        FROM Revistas ORDER BY NEWID();
        """;

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken)) return null;

        return MapComic(reader);
    }


    public async Task<Comic?> GetBySlugAsync(string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var normalizedSlug = ComicSlug.Normalize(slug);
        const string sql = """
        SELECT Codigo, RevistaBR, EdicaoBR, EditoraBR, ditoraEUA, NomeMesBR, AnoRevBR, Preco
        FROM Revistas;
        """;

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var comic = MapComic(reader);
            var comicSlug = ComicSlug.Create(comic.Name, comic.Number);

            if (string.Equals(ComicSlug.Normalize(comicSlug),
                normalizedSlug, StringComparison.OrdinalIgnoreCase))
                return comic;
        }

        return null;
    }

    public async Task<List<Comic>> GetEditionsAsync(string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return [];

        const string sql = """
        SELECT Codigo, RevistaBR, EdicaoBR, EditoraBR, EditoraEUA, NomeMesBR, AnoRevBR, Preco
        FROM Revistas WHERE LTRIM(RTRIM(RevistaBR)) = @Name ORDER BY EdicaoBR ASC, Codigo ASC;
        """;

        var editions = new List<Comic>();

        await using var connection = _factory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 255).Value = name.Trim();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            editions.Add(MapComic(reader));

        return editions;
    }

    private static Comic MapComic(SqlDataReader reader)
    {
        return new Comic
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Number = Convert.ToInt32(reader.GetValue(2)),
            Publisher = reader.GetString(3),
            Licensor = reader.GetString(4),
            ComicMonth = reader.GetString(5),
            ComicYear = Convert.ToInt32(reader.GetValue(6)),
            Price = reader.GetDecimal(7)
        };
    }
}
