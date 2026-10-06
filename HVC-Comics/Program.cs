using HVC_Comics.ComicData;
using HVC_Comics.Configuration;
using HVC_Comics.Data;
using HVC_Comics.Repositories;
using HVC_Comics.Storage;

using System.Globalization;

using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Culture
var culture = new CultureInfo("pt-BR");

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Environment Configuration
var environment = builder.Environment.EnvironmentName;

if (environment.Equals(
"Local",
StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

    var localPlatformConfig =
        OperatingSystem.IsWindows()
            ? "appsettings.Local.Windows.json"
            : "appsettings.Local.Linux.json";

    builder.Configuration.AddJsonFile(
        localPlatformConfig,
        optional: false,
        reloadOnChange: true);


}
else if (environment.Equals(
"AWS",
StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration.AddJsonFile(
    "appsettings.AWS.json",
    optional: false,
    reloadOnChange: true);
}
else if (environment.Equals(
"Azure",
StringComparison.OrdinalIgnoreCase))
{
    builder.Configuration.AddJsonFile(
    "appsettings.Azure.json",
    optional: false,
    reloadOnChange: true);
}

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.Configure<ComicDataOptions>(
builder.Configuration.GetSection("ComicData"));

// Configuration
var comicDataSource =
builder.Configuration["ComicData:Source"]
?.Trim()
.ToLowerInvariant();

var coverProvider =
builder.Configuration["ComicCovers:Provider"]
?.Trim()
.ToLowerInvariant();

// AWS
var requiresAws =
comicDataSource == "s3" ||
coverProvider == "s3";

if (requiresAws)
{
    builder.Services.AddDefaultAWSOptions(
    builder.Configuration.GetAWSOptions());

    builder.Services.AddAWSService<IAmazonS3>();


}

// Comic Data Storage
switch (comicDataSource)
{
    case "json":

        var jsonFile =
            builder.Configuration[
                "ComicData:JsonFile"];

        if (string.IsNullOrWhiteSpace(jsonFile))
        {
            throw new InvalidOperationException(
                "ComicData:JsonFile não configurado.");
        }

        builder.Services.AddSingleton<IComicDataStorage>(
            new LocalComicDataStorage(jsonFile));

        break;

    case "s3":

        var s3Bucket =
            builder.Configuration[
                "ComicData:S3:Bucket"];

        var s3Key =
            builder.Configuration[
                "ComicData:S3:Key"];

        if (string.IsNullOrWhiteSpace(s3Bucket))
        {
            throw new InvalidOperationException(
                "ComicData:S3:Bucket não configurado.");
        }

        if (string.IsNullOrWhiteSpace(s3Key))
        {
            throw new InvalidOperationException(
                "ComicData:S3:Key não configurado.");
        }

        builder.Services.AddScoped<IComicDataStorage>(
            provider =>
                new S3ComicDataStorage(
                    provider.GetRequiredService<IAmazonS3>(),
                    s3Bucket,
                    s3Key));

        break;

    case "azureblob":

        var azureAccountName =
            builder.Configuration[
                "ComicData:AzureBlob:AccountName"];

        var azureContainer =
            builder.Configuration[
                "ComicData:AzureBlob:Container"];

        var azureBlobName =
            builder.Configuration[
                "ComicData:AzureBlob:BlobName"];

        if (string.IsNullOrWhiteSpace(azureAccountName))
        {
            throw new InvalidOperationException(
                "ComicData:AzureBlob:AccountName não configurado.");
        }

        if (string.IsNullOrWhiteSpace(azureContainer))
        {
            throw new InvalidOperationException(
                "ComicData:AzureBlob:Container não configurado.");
        }

        if (string.IsNullOrWhiteSpace(azureBlobName))
        {
            throw new InvalidOperationException(
                "ComicData:AzureBlob:BlobName não configurado.");
        }

        builder.Services.AddSingleton<IComicDataStorage>(
            new AzureBlobComicDataStorage(
                azureAccountName,
                azureContainer,
                azureBlobName));

        break;

    default:

        throw new InvalidOperationException(
            $"Fonte de dados '{comicDataSource}' não suportada. " +
            "Valores válidos: Json, S3, AzureBlob.");


}

// Comic Cover Storage
switch (coverProvider)
{
    case "local":

        var coversPath =
            builder.Configuration[
                "ComicCovers:Path"];

        if (string.IsNullOrWhiteSpace(coversPath))
        {
            throw new InvalidOperationException(
                "ComicCovers:Path não configurado.");
        }

        builder.Services.AddSingleton<IComicCoverStorage>(
            new LocalComicCoverStorage(
                coversPath));

        break;

    case "s3":

        var coverS3Bucket =
            builder.Configuration[
                "ComicCovers:S3:Bucket"];

        var coverS3Prefix =
            builder.Configuration[
                "ComicCovers:S3:Prefix"];

        if (string.IsNullOrWhiteSpace(coverS3Bucket))
        {
            throw new InvalidOperationException(
                "ComicCovers:S3:Bucket não configurado.");
        }

        if (string.IsNullOrWhiteSpace(coverS3Prefix))
        {
            throw new InvalidOperationException(
                "ComicCovers:S3:Prefix não configurado.");
        }

        builder.Services.AddScoped<IComicCoverStorage>(
            provider =>
                new S3ComicCoverStorage(
                    provider.GetRequiredService<IAmazonS3>(),
                    coverS3Bucket,
                    coverS3Prefix));

        break;

    case "azureblob":

        var coverAzureAccountName =
            builder.Configuration[
                "ComicCovers:AzureBlob:AccountName"];

        var coverAzureContainer =
            builder.Configuration[
                "ComicCovers:AzureBlob:Container"];

        var coverAzurePrefix =
            builder.Configuration[
                "ComicCovers:AzureBlob:Prefix"];

        if (string.IsNullOrWhiteSpace(
                coverAzureAccountName))
        {
            throw new InvalidOperationException(
                "ComicCovers:AzureBlob:AccountName não configurado.");
        }

        if (string.IsNullOrWhiteSpace(
                coverAzureContainer))
        {
            throw new InvalidOperationException(
                "ComicCovers:AzureBlob:Container não configurado.");
        }

        if (string.IsNullOrWhiteSpace(
                coverAzurePrefix))
        {
            throw new InvalidOperationException(
                "ComicCovers:AzureBlob:Prefix não configurado.");
        }

        builder.Services.AddSingleton<IComicCoverStorage>(
            new AzureBlobComicCoverStorage(
                coverAzureAccountName,
                coverAzureContainer,
                coverAzurePrefix));

        break;

    default:

        throw new InvalidOperationException(
            $"Provider de capas '{coverProvider}' não suportado. " +
            "Valores válidos: Local, S3, AzureBlob.");


}

// Repository
switch (comicDataSource)
{
    case "json":
    case "s3":
    case "azureblob":

        builder.Services.AddScoped<JsonComicRepository>();

        builder.Services.AddScoped<IComicRepository>(
            provider =>
                provider.GetRequiredService<JsonComicRepository>());

        break;

    case "sqlserver":

        builder.Services.AddScoped<SqlServerConnectionFactory>();

        builder.Services.AddScoped<
            IComicRepository,
            SqlServerComicRepository>();

        break;

    case "mysql":

        builder.Services.AddScoped<MySqlConnectionFactory>();

        builder.Services.AddScoped<
            IComicRepository,
            MySqlComicRepository>();

        break;

    case "postgresql":
    case "postgres":

        builder.Services.AddScoped<PostgreSqlConnectionFactory>();

        builder.Services.AddScoped<
            IComicRepository,
            PostgreSqlComicRepository>();

        break;

    default:

        throw new InvalidOperationException(
            $"Fonte de dados '{comicDataSource}' não suportada.");


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

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
.WithStaticAssets();

app.Run();
