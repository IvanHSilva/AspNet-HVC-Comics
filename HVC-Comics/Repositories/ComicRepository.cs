using Microsoft.Data.SqlClient;
using HVC_Comics.Data;
using HVC_Comics.Models;

namespace HVC_Comics.Repositories;

public class ComicRepository(SqlServerConnectionFactory factory)
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public List<Comic> GetAll()
    {
        var comics = new List<Comic>();

        using var connection = _factory.CreateConnection();

        connection.Open();

        string sql = @" SELECT Codigo, RevistaBR, EdicaoBR, Titulo, Preco
            FROM Revistas
            ORDER BY Codigo";

        using var command = new SqlCommand(sql, connection);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            comics.Add(new Comic
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Number = reader.GetInt16(2),
                ComicTitle = reader.GetString(3),
                Price = reader.GetDecimal(4)
            });
        }

        return comics;
    }
}
