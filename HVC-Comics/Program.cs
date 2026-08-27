using HVC_Comics.Data;
using HVC_Comics.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<SqlServerConnection>();
builder.Services.AddScoped<SqlServerConnectionFactory>();
//builder.Services.AddScoped<ComicRepository>();
var comicSource = builder.Configuration["ComicData:Source"];

if (string.Equals(comicSource, "Json", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IComicRepository, JsonComicRepository>();
}
else
{
    builder.Services.AddScoped<IComicRepository, ComicRepository>();
}

var app = builder.Build();

// Configure OS file paths
var platformConfig = OperatingSystem.IsWindows()
    ? "appsettings.Windows.json"
    : "appsettings.Linux.json";

builder.Configuration.AddJsonFile(
    platformConfig,
    optional: false,
    reloadOnChange: true);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
