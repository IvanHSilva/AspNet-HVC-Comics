using HVC_Comics.Data;
using HVC_Comics.Models;
using Microsoft.Data.SqlClient;

namespace HVC_Comics.Repositories;

public class ComicRepository(SqlServerConnectionFactory factory) : IComicRepository
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public PaginationResult<Comic> GetPaged(int page = 1, int pageSize = 50)
    {
        var result = new PaginationResult<Comic>
        {
            CurrentPage = page,
            PageSize = pageSize
        };

        using var connection = _factory.CreateConnection();

        connection.Open();

        // Total records
        using (var countCommand = new SqlCommand(
            "SELECT COUNT(*) FROM Revistas",
            connection))
        {
            result.TotalRecords = (int)countCommand.ExecuteScalar();
        }

        int offset = (page - 1) * pageSize;

        string sql = @"
        SELECT Codigo, RevistaBR, EdicaoBR, EditoraBR, EditoraEUA, NomeMesBR, AnoREvBR, Preco
        FROM Revistas
        ORDER BY Codigo
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Offset", offset);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Items.Add(new Comic
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Number = reader.GetInt16(2),
                Publisher = reader.GetString(3),
                Licensor = reader.GetString(4),
                ComicMonth = reader.GetString(5),
                ComicYear = reader.GetInt16(6),
                Price = reader.GetDecimal(7)
            });
        }

        return result;
    }
}
