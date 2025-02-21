using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using MvcMovie.Models;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Retrieve environment variables
string dbServer = Environment.GetEnvironmentVariable("PROD_DB_SERVER");
string dbName = Environment.GetEnvironmentVariable("DB_NAME");
string dbUser = Environment.GetEnvironmentVariable("PROD_DB_USER");
string dbPassword = Environment.GetEnvironmentVariable("PROD_DB_PASSWORD");

// Construct the connection string using actual environment variables
string connectionString = $"Server={dbServer};Initial Catalog={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";

builder.Services.AddDbContext<MvcMovieContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add this if it's not already present
builder.WebHost.ConfigureKestrel(serverOptions =>
{

    serverOptions.ListenAnyIP(80);
    serverOptions.ConfigureHttpsDefaults(configureOptions =>
    {
        configureOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MvcMovieContext>();

    context.Database.Migrate();

    SeedData.Initialize(services);
}

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
