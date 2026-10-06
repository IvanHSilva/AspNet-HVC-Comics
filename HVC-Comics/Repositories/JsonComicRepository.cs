using System.Globalization;
using System.Text.Json;

using HVC_Comics.ComicData;
using HVC_Comics.Models;

using Microsoft.Extensions.Caching.Memory;

namespace HVC_Comics.Repositories;

public class JsonComicRepository(
    IComicDataStorage dataStorage,
    IMemoryCache cache,
    ILogger<JsonComicRepository> logger) : IComicRepository
{
    private const string CacheKey = "comics";

    private readonly IComicDataStorage _dataStorage = dataStorage;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<JsonComicRepository> _logger = logger;

    public async Task<PaginationResult<Comic>> GetPagedAsync(
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var comics = await GetComicsAsync(cancellationToken);

        var totalRecords = comics.Count;

        var totalPages = totalRecords == 0
            ? 1
            : (int)Math.Ceiling(
                (double)totalRecords / pageSize);

        page = Math.Min(page, totalPages);

        return new PaginationResult<Comic>
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            DataSource = "JSON",
            Items =
            [
                .. comics
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
            ]
        };
    }

    public async Task<Comic?> GetRandomAsync(
        CancellationToken cancellationToken = default)
    {
        var comics = await GetComicsAsync(cancellationToken);

        if (comics.Count == 0)
        {
            return null;
        }

        var randomId =
            comics[Random.Shared.Next(comics.Count)].Id;

        return comics.FirstOrDefault(
            comic => comic.Id == randomId);
    }

    public async Task<Comic?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var comics = await GetComicsAsync(cancellationToken);

        return comics.FirstOrDefault(
            comic => comic.Id == id);
    }

    private async Task<List<Comic>> GetComicsAsync(
        CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(
                CacheKey,
                out List<Comic>? comics) &&
            comics is not null)
        {
            return comics;
        }

        comics = await LoadComicsAsync(
            cancellationToken);

        _cache.Set(
            CacheKey,
            comics,
            TimeSpan.FromMinutes(10));

        return comics;
    }

    private async Task<List<Comic>> LoadComicsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream =
                await _dataStorage.GetAsync(
                    cancellationToken);

            if (stream is null)
            {
                _logger.LogWarning(
                    "Não foi possível localizar Comics.json.");

                return [];
            }

            var source =
                await JsonSerializer.DeserializeAsync<
                    List<ComicJson>>(
                        stream,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        },
                        cancellationToken) ?? [];

            return
            [
                .. source
                    .Select(ToComic)
                    .OrderBy(comic => comic.Id)
            ];
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "O arquivo Comics.json é inválido.");

            return [];
        }
        catch (IOException exception)
        {
            _logger.LogWarning(
                exception,
                "Não foi possível ler Comics.json.");

            return [];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "ERRO AO CARREGAR COMICS.JSON: {ExceptionType} - {Message}",
                exception.GetType().FullName,
                exception.Message);

            throw;
        }
    }

    private static DateOnly ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

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

            ComicCover = string.Empty,

            ComicNumber = source.CapaEdicaoEUA,

            Period = source.Fase,
            Event = source.Evento,

            Conservation = source.Conservacao,
            Problem1 = source.Problema1,
            Problem2 = source.Problema2,

            RegDate = ParseDate(source.DataCadastro),

            // O Comics.json utiliza true/false diretamente.
            IsLastEdition = source.UltimaEdicao,
            HaveMail = source.Correio,
            HaveChecklist = source.Checklist,
            IsBook = source.Encadernado,
            IsReedition = source.Reedicao,
            IsCrossover = source.Crossover,
            IsPhisic = source.Fisica,
            IsDigital = source.Digital,
            IsBlackWhite = source.SemCores,

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

        // O JSON utiliza booleanos true/false.
        public bool UltimaEdicao { get; set; }
        public bool Correio { get; set; }
        public bool Checklist { get; set; }
        public bool Encadernado { get; set; }
        public bool Reedicao { get; set; }
        public bool Crossover { get; set; }
        public bool Fisica { get; set; }
        public bool Digital { get; set; }
        public bool SemCores { get; set; }

        public string Servidor { get; set; } = string.Empty;
    }
}
