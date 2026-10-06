using HVC_Comics.Data;
using HVC_Comics.Models;

using Npgsql;

namespace HVC_Comics.Repositories;

public class PostgreSqlComicRepository(
PostgreSqlConnectionFactory factory) : IComicRepository
{
    private readonly PostgreSqlConnectionFactory _factory = factory;

    public async Task<PaginationResult<Comic>> GetPagedAsync(
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = new PaginationResult<Comic>
        {
            CurrentPage = page,
            PageSize = pageSize,
            DataSource = "PostgreSQL"
        };

        await using var connection =
            _factory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        const string countSql = """
        SELECT COUNT(*)
        FROM "public"."Revistas";
        """;

        await using (var countCommand = new NpgsqlCommand(
            countSql,
            connection))
        {
            result.TotalRecords =
                Convert.ToInt32(
                    await countCommand.ExecuteScalarAsync(
                        cancellationToken));
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
            "Codigo",
            "RevistaBR",
            "EdicaoBR",
            "Historias",
            "Materias",
            "MesRevBR",
            "AnoRevBR",
            "DataRevBR",
            "NomeMesBR",
            "Paginas",
            "EditoraBR",
            "EditoraEUA",
            "Formato",
            "Moeda",
            "Preco",
            "Periodicidade",
            "SituacaoRev",
            "Papel",
            "Encadernacao",
            "TipoCapa",
            "PersonagensCapa",
            "PersonagensContracapa",
            "Titulo",
            "Chamada",
            "CapaRevistaEUA",
            "CapaEdicaoEUA",
            "Fase",
            "Evento",
            "Conservacao",
            "Problema1",
            "Problema2",
            "DataCadastro",
            "UltimaEdicao",
            "Correio",
            "Checklist",
            "Encadernado",
            "Reedicao",
            "Crossover",
            "Fisica",
            "Digital",
            "SemCores",
            "Servidor"
        FROM "public"."Revistas"
        ORDER BY "Codigo"
        LIMIT @PageSize
        OFFSET @Offset;
        """;

        await using var command = new NpgsqlCommand(
            sql,
            connection);

        command.Parameters.AddWithValue(
            "PageSize",
            pageSize);

        command.Parameters.AddWithValue(
            "Offset",
            offset);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        while (await reader.ReadAsync(
            cancellationToken))
        {
            result.Items.Add(new Comic
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Number = reader.GetInt16(2),

                Stories = reader.GetInt16(3),
                Articles = reader.GetInt16(4),

                ComicMonth = reader.GetString(8),
                ComicYear = reader.GetInt16(6),
                ComicDate = DateOnly.FromDateTime(
                    reader.GetDateTime(7)),

                Pages = reader.GetInt16(9),

                Publisher = reader.GetString(10),
                Licensor = reader.GetString(11),

                Format = reader.GetString(12),
                Coin = reader.GetString(13),
                Price = reader.GetDecimal(14),

                Frequency = reader.GetString(15),
                ComicSituation = reader.GetString(16),

                PaperType = reader.GetString(17),
                Binding = reader.GetString(18),
                CoverType = reader.GetString(19),

                CoverChar = reader.GetString(20),
                ComicTitle = reader.GetString(22),
                ComicCall = reader.GetString(23),
                ComicCover = reader.GetString(24),
                ComicNumber = reader.GetInt16(25),

                Period = reader.GetString(26),
                Event = reader.GetString(27),

                Conservation = reader.GetString(28),
                Problem1 = reader.GetString(29),
                Problem2 = reader.GetString(30),

                RegDate = DateOnly.FromDateTime(
                    reader.GetDateTime(31)),

                IsLastEdition = reader.GetBoolean(32),
                HaveMail = reader.GetBoolean(33),
                HaveChecklist = reader.GetBoolean(34),
                IsBook = reader.GetBoolean(35),
                IsReedition = reader.GetBoolean(36),
                IsCrossover = reader.GetBoolean(37),
                IsPhisic = reader.GetBoolean(38),
                IsDigital = reader.GetBoolean(39),
                IsBlackWhite = reader.GetBoolean(40),

                RegServer = reader.GetString(41)
            });
        }

        return result;
    }

    public async Task<Comic?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT
            "Codigo",
            "RevistaBR",
            "EdicaoBR",
            "Historias",
            "Materias",
            "MesRevBR",
            "AnoRevBR",
            "DataRevBR",
            "NomeMesBR",
            "Paginas",
            "EditoraBR",
            "EditoraEUA",
            "Formato",
            "Moeda",
            "Preco",
            "Periodicidade",
            "SituacaoRev",
            "Papel",
            "Encadernacao",
            "TipoCapa",
            "PersonagensCapa",
            "PersonagensContracapa",
            "Titulo",
            "Chamada",
            "CapaRevistaEUA",
            "CapaEdicaoEUA",
            "Fase",
            "Evento",
            "Conservacao",
            "Problema1",
            "Problema2",
            "DataCadastro",
            "UltimaEdicao",
            "Correio",
            "Checklist",
            "Encadernado",
            "Reedicao",
            "Crossover",
            "Fisica",
            "Digital",
            "SemCores",
            "Servidor"
        FROM "public"."Revistas"
        WHERE "Codigo" = @Id
        LIMIT 1;
        """;

        await using var connection =
            _factory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            sql,
            connection);

        command.Parameters.AddWithValue(
            "Id",
            id);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return MapComic(reader);
    }

    public async Task<Comic?> GetRandomAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT
            "Codigo",
            "RevistaBR",
            "EdicaoBR",
            "Historias",
            "Materias",
            "MesRevBR",
            "AnoRevBR",
            "DataRevBR",
            "NomeMesBR",
            "Paginas",
            "EditoraBR",
            "EditoraEUA",
            "Formato",
            "Moeda",
            "Preco",
            "Periodicidade",
            "SituacaoRev",
            "Papel",
            "Encadernacao",
            "TipoCapa",
            "PersonagensCapa",
            "PersonagensContracapa",
            "Titulo",
            "Chamada",
            "CapaRevistaEUA",
            "CapaEdicaoEUA",
            "Fase",
            "Evento",
            "Conservacao",
            "Problema1",
            "Problema2",
            "DataCadastro",
            "UltimaEdicao",
            "Correio",
            "Checklist",
            "Encadernado",
            "Reedicao",
            "Crossover",
            "Fisica",
            "Digital",
            "SemCores",
            "Servidor"
        FROM "public"."Revistas"
        ORDER BY RANDOM()
        LIMIT 1;
        """;

        await using var connection =
            _factory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            sql,
            connection);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        if (!await reader.ReadAsync(
            cancellationToken))
        {
            return null;
        }

        return MapComic(reader);
    }

    private static Comic MapComic(
        NpgsqlDataReader reader)
    {
        return new Comic
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Number = reader.GetInt16(2),

            Stories = reader.GetInt16(3),
            Articles = reader.GetInt16(4),

            ComicMonth = reader.GetString(8),
            ComicYear = reader.GetInt16(6),
            ComicDate = DateOnly.FromDateTime(
                reader.GetDateTime(7)),

            Pages = reader.GetInt16(9),

            Publisher = reader.GetString(10),
            Licensor = reader.GetString(11),

            Format = reader.GetString(12),
            Coin = reader.GetString(13),
            Price = reader.GetDecimal(14),

            Frequency = reader.GetString(15),
            ComicSituation = reader.GetString(16),

            PaperType = reader.GetString(17),
            Binding = reader.GetString(18),
            CoverType = reader.GetString(19),

            CoverChar = reader.GetString(20),
            ComicTitle = reader.GetString(22),
            ComicCall = reader.GetString(23),
            ComicCover = reader.GetString(24),
            ComicNumber = reader.GetInt16(25),

            Period = reader.GetString(26),
            Event = reader.GetString(27),

            Conservation = reader.GetString(28),
            Problem1 = reader.GetString(29),
            Problem2 = reader.GetString(30),

            RegDate = DateOnly.FromDateTime(
                reader.GetDateTime(31)),

            IsLastEdition = reader.GetBoolean(32),
            HaveMail = reader.GetBoolean(33),
            HaveChecklist = reader.GetBoolean(34),
            IsBook = reader.GetBoolean(35),
            IsReedition = reader.GetBoolean(36),
            IsCrossover = reader.GetBoolean(37),
            IsPhisic = reader.GetBoolean(38),
            IsDigital = reader.GetBoolean(39),
            IsBlackWhite = reader.GetBoolean(40),

            RegServer = reader.GetString(41)
        };
    }
}
