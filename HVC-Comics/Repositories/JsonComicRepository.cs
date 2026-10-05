using System.Text.Json;
using HVC_Comics.Models;
using Microsoft.Extensions.Caching.Memory;

using System.Globalization;

namespace HVC_Comics.Repositories;

public class JsonComicRepository(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IMemoryCache cache,
    ILogger<JsonComicRepository> logger) : IComicRepository
{
    private readonly IConfiguration _configuration = configuration;
    private const string CacheKey = "comics";

    private readonly IWebHostEnvironment _environment = environment;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<JsonComicRepository> _logger = logger;

    public PaginationResult<Comic> GetPaged(int page = 1, int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var comics = _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return LoadComics();
        }) ?? [];

        var totalRecords = comics.Count;
        var totalPages = totalRecords == 0
            ? 1
            : (int)Math.Ceiling((double)totalRecords / pageSize);

        page = Math.Min(page, totalPages);

        return new PaginationResult<Comic>
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            DataSource = "JSON",
            Items = [.. comics
                .Skip((page - 1) * pageSize)
                .Take(pageSize)]
        };
    }

    public Comic? GetRandom()
    {
        var comics = GetComics();

        if (comics.Count == 0)
        {
            return null;
        }

        var randomId = comics[
            Random.Shared.Next(comics.Count)
        ].Id;

        return GetById(randomId);
    }

    public Comic? GetById(int id)
    {
        var comics = GetComics();

        return comics.FirstOrDefault(
            comic => comic.Id == id);
    }

    private List<Comic> GetComics()
    {
        return _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(10);

            return LoadComics();
        }) ?? [];
    }

    private List<Comic> LoadComics()
    {
        var configuredFile = _configuration["ComicData:JsonFile"];

        if (string.IsNullOrWhiteSpace(configuredFile))
        {
            _logger.LogWarning(
                "O caminho do arquivo de backup JSON não foi configurado.");

            return [];
        }

        var file = Path.IsPathRooted(configuredFile)
            ? configuredFile
            : Path.Combine(_environment.ContentRootPath, configuredFile);

        if (!File.Exists(file))
        {
            _logger.LogWarning(
                "O arquivo de backup JSON não foi encontrado: {File}",
                file);

            return [];
        }

        try
        {
            var json = File.ReadAllText(file);

            var source = JsonSerializer.Deserialize<List<ComicJson>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? [];

            return [.. source
            .Select(ToComic)
            .OrderBy(comic => comic.Id)];
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "O arquivo de backup JSON é inválido: {File}",
                file);

            return [];
        }
        catch (IOException exception)
        {
            _logger.LogWarning(
                exception,
                "Não foi possível ler o arquivo JSON: {File}",
                file);

            return [];
        }
    }

    private static bool ToBool(string value)
    {
        return value == "1";
    }

    private static DateOnly ParseDate(string value)
    {
        return DateOnly.TryParse(
            value,
            new CultureInfo("pt-BR"),
            DateTimeStyles.None,
            out var date)
            ? date
            : default;
    }

    private static Comic ToComic(ComicJson source)
    {
        return new Comic
        {
            Id = source.Codigo,
            Name = source.RevistaBR,
            Number = source.EdicaoBR,

            Stories = source.Historias,
            Articles = source.Materias,

            ComicMonth = source.NomeMesBR,
            ComicYear = source.AnoRevBR,

            ComicDate = ParseDate(source.DataRevBR),

            Pages = source.Paginas,

            Publisher = source.EditoraBR,
            Licensor = source.EditoraEUA,

            Format = source.Formato,
            Coin = source.Moeda,
            Price = source.Preco,

            Frequency = source.Periodicidade,
            ComicSituation = source.SituacaoRev,

            PaperType = source.Papel,
            Binding = source.Encadernacao,
            CoverType = source.TipoCapa,

            CoverChar = source.PersonagensCapa,
            ComicTitle = source.Titulo,
            ComicCall = source.Chamada,

            // Ainda não existe imagem da capa no JSON.
            ComicCover = string.Empty,

            ComicNumber = source.CapaEdicaoEUA,

            Period = source.Fase,
            Event = source.Evento,

            Conservation = source.Conservacao,
            Problem1 = source.Problema1,
            Problem2 = source.Problema2,

            RegDate = ParseDate(source.DataCadastro),

            IsLastEdition = ToBool(source.UltimaEdicao),
            HaveMail = ToBool(source.Correio),
            HaveChecklist = ToBool(source.Checklist),
            IsBook = ToBool(source.Encadernado),
            IsReedition = ToBool(source.Reedicao),
            IsCrossover = ToBool(source.Crossover),
            IsPhisic = ToBool(source.Fisica),
            IsDigital = ToBool(source.Digital),
            IsBlackWhite = ToBool(source.SemCores),

            RegServer = source.Servidor
        };
    }

    private sealed class ComicJson
    {
        public int Codigo { get; set; }
        public string RevistaBR { get; set; } = string.Empty;
        public int EdicaoBR { get; set; }

        public int Historias { get; set; }
        public int Materias { get; set; }

        public int MesRevBR { get; set; }
        public int AnoRevBR { get; set; }
        public string DataRevBR { get; set; } = string.Empty;
        public string NomeMesBR { get; set; } = string.Empty;

        public int Paginas { get; set; }

        public string EditoraBR { get; set; } = string.Empty;
        public string EditoraEUA { get; set; } = string.Empty;

        public string Formato { get; set; } = string.Empty;
        public string Moeda { get; set; } = string.Empty;
        public decimal Preco { get; set; }

        public string Periodicidade { get; set; } = string.Empty;
        public string SituacaoRev { get; set; } = string.Empty;

        public string Papel { get; set; } = string.Empty;
        public string Encadernacao { get; set; } = string.Empty;
        public string TipoCapa { get; set; } = string.Empty;

        public string PersonagensCapa { get; set; } = string.Empty;
        public string PersonagensContracapa { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;
        public string Chamada { get; set; } = string.Empty;

        public string CapaRevistaEUA { get; set; } = string.Empty;
        public int CapaEdicaoEUA { get; set; }

        public string Fase { get; set; } = string.Empty;
        public string Evento { get; set; } = string.Empty;

        public string Conservacao { get; set; } = string.Empty;
        public string Problema1 { get; set; } = string.Empty;
        public string Problema2 { get; set; } = string.Empty;

        public string DataCadastro { get; set; } = string.Empty;

        public string UltimaEdicao { get; set; } = string.Empty;
        public string Correio { get; set; } = string.Empty;
        public string Checklist { get; set; } = string.Empty;
        public string Encadernado { get; set; } = string.Empty;
        public string Reedicao { get; set; } = string.Empty;
        public string Crossover { get; set; } = string.Empty;
        public string Fisica { get; set; } = string.Empty;
        public string Digital { get; set; } = string.Empty;
        public string SemCores { get; set; } = string.Empty;

        public string Servidor { get; set; } = string.Empty;
    }
}