using HVC_Comics.Data;
using HVC_Comics.Models;
using Microsoft.Data.SqlClient;

namespace HVC_Comics.Repositories;

public class ComicRepository(SqlServerConnectionFactory factory)
{
    private readonly SqlServerConnectionFactory _factory = factory;

    public List<Comic> GetPaged(int page = 1, int pageSize = 50)
    {
        var comics = new List<Comic>();

        int offset = (page - 1) * pageSize;

        using var connection = _factory.CreateConnection();

        connection.Open();

        string sql = @"
            SELECT Codigo, RevistaBR, EdicaoBR, Historias, Materias, DataRevBR, 
            Paginas,EditoraBR, EditoraEUA, Preco, Titulo, PersonagensCapa 
            FROM Revistas
            ORDER BY Codigo
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
        ";

        using var command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Offset", offset);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            comics.Add(new Comic
            {
                Id = reader.GetInt32(reader.GetOrdinal("Codigo")),
                Name = reader.GetString(
                    reader.GetOrdinal("RevistaBR")),
                Number = reader.GetInt16(
                    reader.GetOrdinal("EdicaoBR")),
                Stories = reader.GetInt16(
                    reader.GetOrdinal("Historias")),
                Articles = reader.GetInt16(
                    reader.GetOrdinal("Materias")),
                ComicDate = DateOnly.FromDateTime(
                    reader.GetDateTime(
                        reader.GetOrdinal("DataRevBR"))),
                Pages = reader.GetInt16(
                    reader.GetOrdinal("Paginas")),
                Publisher = reader.GetString(
                    reader.GetOrdinal("EditoraBR")),
                Licensor = reader.GetString(
                    reader.GetOrdinal("EditoraEUA")),
                Price = reader.GetDecimal(
                    reader.GetOrdinal("Preco")),
                ComicTitle = reader.GetString(
                    reader.GetOrdinal("Titulo")),
                CoverChar = reader.GetString(
                    reader.GetOrdinal("PersonagensCapa"))
            });
        }

        return comics;
    }
}
