using Scalar.AspNetCore;
using Serilog;
using Shared.Middlewares;

namespace FileService.Web.Configurations;

public static class AppExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseExceptionMiddleware();
        app.UseSerilogRequestLogging();

        app.MapOpenApi();
        app.MapScalarApiReference();

        return app;
    }
}