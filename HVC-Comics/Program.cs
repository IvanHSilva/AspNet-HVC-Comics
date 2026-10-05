using HVC_Comics.Configuration;
using HVC_Comics.Data;
using HVC_Comics.Repositories;

using System.Globalization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Culture info
var culture = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// OS Configuration
var platformConfig = OperatingSystem.IsWindows()
    ? "appsettings.Windows.json"
    : "appsettings.Linux.json";

builder.Configuration.AddJsonFile(
    platformConfig,
    optional: true,
    reloadOnChange: true);

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// Services
builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();

builder.Services.Configure<ComicDataOptions>(
    builder.Configuration.GetSection("ComicData"));

// Data Source
var comicSource = builder.Configuration["ComicData:Source"];

switch (comicSource?.Trim().ToLowerInvariant())
{
    case "json":

        builder.Services.AddScoped<JsonComicRepository>();

        builder.Services.AddScoped<IComicRepository>(
            provider =>
                provider.GetRequiredService<JsonComicRepository>());

        break;

    case "sqlserver":

        builder.Services.AddScoped<
            SqlServerConnectionFactory>();

        builder.Services.AddScoped<
            IComicRepository,
            SqlServerComicRepository>();

        break;

    case "mysql":

        builder.Services.AddScoped<
                    MySqlConnectionFactory>();

        builder.Services.AddScoped<
            IComicRepository,
            MySqlComicRepository>();

        break;

    case "postgresql":
    case "postgres":
        builder.Services.AddScoped<PostgreSqlConnectionFactory>();
        builder.Services.AddScoped<IComicRepository, PostgreSqlComicRepository>();
        break;

    default:

        throw new InvalidOperationException(
            $"Fonte de dados '{comicSource}' não suportada. " +
            "Valores válidos: Json, SqlServer, MySql, PostgreSql.");
}

// Application
var app = builder.Build();

// HTTP Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

var coversPath = builder.Configuration["ComicCovers:Path"];

if (!string.IsNullOrWhiteSpace(coversPath) &&
    Directory.Exists(coversPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(coversPath),
        RequestPath = "/capas"
    });
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
