using System.Globalization;
using FileService.Infrastructure.Postgres.Extensions;
using FileService.Web.Configurations;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Design", LogEventLevel.Warning)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting file service");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    string envName = builder.Environment.EnvironmentName;

    builder.Configuration.AddJsonFile($"appsettings.{envName}.json", true, true);

    builder.Configuration.AddEnvironmentVariables(); // для поддержки переменных окружения .env

    builder.Services.AddConfiguration(builder.Configuration);

    WebApplication app = builder.Build();
    
    if (app.Environment.IsDevelopment())
    {
        await app.ApplyMigrationsAsync();
    }
    
    app.Configure();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}