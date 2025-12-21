using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DirectoryDbContext>(_ =>
    new DirectoryDbContext(builder.Configuration.GetConnectionString("directory_service")!));

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DirectoryService",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Education"
        }
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Directory Service API"));
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();