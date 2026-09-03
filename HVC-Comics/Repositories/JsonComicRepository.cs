using System.Text.Json;
using HVC_Comics.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HVC_Comics.Repositories;

public class JsonComicRepository(
    IConfiguration configuration,
    IWebHostEnvironment environment,
    IMemoryCache cache,
    ILogger<JsonComicRepository> logger) : IComicRepository
{
    private readonly IConfiguration _configuration = configuration;
    private const string CacheKey = "comic-json-backup";

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

    private List<Comic> LoadComics()
    {
        var file = _configuration["ComicData:JsonFile"];

        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
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

    private static Comic ToComic(ComicJson source)
    {
        return new Comic
        {
            Id = source.Codigo,
            Name = source.RevistaBR,
            Number = source.EdicaoBR,
            Publisher = source.EditoraBR,
            Licensor = source.EditoraEUA,
            ComicMonth = source.NomeMesBR,
            ComicYear = source.AnoRevBR,
            Price = source.Preco
        };
    }

    private sealed class ComicJson
    {
        public int Codigo { get; set; }
        public string RevistaBR { get; set; } = string.Empty;
        public int EdicaoBR { get; set; }
        public string EditoraBR { get; set; } = string.Empty;
        public string EditoraEUA { get; set; } = string.Empty;
        public string NomeMesBR { get; set; } = string.Empty;
        public int AnoRevBR { get; set; }
        public decimal Preco { get; set; }
    }
}