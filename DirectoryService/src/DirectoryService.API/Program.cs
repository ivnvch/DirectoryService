using DirectoryService.API;
using DirectoryService.API.Middlewares;
using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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

namespace DirectoryService.API
{
    public partial class Program;
}