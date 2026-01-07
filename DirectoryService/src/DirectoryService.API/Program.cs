using DirectoryService.API;
using DirectoryService.API.Middlewares;
using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Microsoft.OpenApi.Models;
using DirectoryService.API.Middlewares;
using DirectoryService.Shared.Errors;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<DirectoryDbContext>(_ =>
    new DirectoryDbContext(builder.Configuration.GetConnectionString("directory_service")!));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddConfiguration(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Directory Service API"));
}

app.UseSerilogRequestLogging();

app.UseExceptionMiddleware();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();