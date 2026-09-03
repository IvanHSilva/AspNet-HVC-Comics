using HVC_Comics.Data;
using HVC_Comics.Models;
using Microsoft.Data.SqlClient;

namespace HVC_Comics.Repositories;

public class SqlServerComicRepository(
    SqlServerConnectionFactory factory) : IComicRepository
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public PaginationResult<Comic> GetPaged(
        int page = 1,
        int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = new PaginationResult<Comic>
        {
            CurrentPage = page,
            PageSize = pageSize,
            DataSource = "SQL Server"
        };

        using var connection = _factory.CreateConnection();

        connection.Open();

        // Total de registros
        using (var countCommand = new SqlCommand(
            "SELECT COUNT(*) FROM Revistas",
            connection))
        {
            result.TotalRecords =
                Convert.ToInt32(countCommand.ExecuteScalar());
        }

        var totalPages = result.TotalRecords == 0
            ? 1
            : (int)Math.Ceiling(
                (double)result.TotalRecords / pageSize);

        page = Math.Min(page, totalPages);
        result.CurrentPage = page;

        var offset = (page - 1) * pageSize;

        const string sql = """
            SELECT
                Codigo,
                RevistaBR,
                EdicaoBR,
                EditoraBR,
                EditoraEUA,
                NomeMesBR,
                AnoREvBR,
                Preco
            FROM Revistas
            ORDER BY Codigo
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@Offset",
            System.Data.SqlDbType.Int).Value = offset;

        command.Parameters.Add(
            "@PageSize",
            System.Data.SqlDbType.Int).Value = pageSize;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Items.Add(new Comic
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Number = Convert.ToInt32(reader.GetValue(2)),
                Publisher = reader.GetString(3),
                Licensor = reader.GetString(4),
                ComicMonth = reader.GetString(5),
                ComicYear = Convert.ToInt32(reader.GetValue(6)),
                Price = reader.GetDecimal(7)
            });
        }

        return result;
    }
}
