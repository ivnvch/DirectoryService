using DirectoryService.API.Middlewares;
using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Microsoft.OpenApi.Models;
using DirectoryService.API.Middlewares;
using DirectoryService.Shared.Errors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
        {
            if (schema.Properties.TryGetValue("errors", out var errorsProp))
            {
                errorsProp.Items.Reference = new OpenApiReference
                {
                    Type = ReferenceType.Schema,
                    Id = "Error"
                };
            }
        }
        return Task.CompletedTask;
    });
});

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

app.UseExceptionMiddleware();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();