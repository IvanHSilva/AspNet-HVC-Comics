using HVC_Comics.Configuration;
using HVC_Comics.Data;
using HVC_Comics.Repositories;

var builder = WebApplication.CreateBuilder(args);

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

        builder.Services.AddScoped<
            IComicRepository,
            JsonComicRepository>();

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

        builder.Services.AddScoped<
            IComicRepository,
            PostgreSqlComicRepository>();

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

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
