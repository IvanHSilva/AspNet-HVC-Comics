using HVC_Comics.Data;
using HVC_Comics.Models;
using MySqlConnector;

namespace HVC_Comics.Repositories;

public class MySqlComicRepository(
    MySqlConnectionFactory factory) : IComicRepository
{
    private readonly MySqlConnectionFactory _factory = factory;

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
            DataSource = "MySQL"
        };

        using var connection = _factory.CreateConnection();

        connection.Open();

        using (var countCommand = new MySqlCommand(
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
            LIMIT @PageSize OFFSET @Offset;
            """;

        using var command = new MySqlCommand(
            sql,
            connection);

        command.Parameters.Add(
            "@PageSize",
            MySqlDbType.Int32).Value = pageSize;

        command.Parameters.Add(
            "@Offset",
            MySqlDbType.Int32).Value = offset;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Items.Add(new Comic
            {
                Id = Convert.ToInt32(reader.GetValue(0)),
                Name = reader.GetString(1),
                Number = Convert.ToInt32(reader.GetValue(2)),
                Publisher = reader.GetString(3),
                Licensor = reader.GetString(4),
                ComicMonth = reader.GetString(5),
                ComicYear = Convert.ToInt32(reader.GetValue(6)),
                Price = Convert.ToDecimal(reader.GetValue(7))
            });
        }

        return result;
    }
}
