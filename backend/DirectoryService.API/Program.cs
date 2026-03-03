using DirectoryService.API;
using DirectoryService.API.Middlewares;
using DirectoryService.Application;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.BackgroundServices;
using DirectoryService.Infrastructure.Seeding;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<DeleteUnActiveDepartmentOptions>(builder.Configuration.GetSection("DeleteUnActiveDepartmentOptions"));
builder.Services.AddHostedService<DeleteUnActiveDepartmentService>();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors();

builder.Services.AddConfiguration(builder.Configuration);

builder.Services.AddScoped<ISeeder, DirectorySeeder>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Directory Service API"));

    /*if (args.Contains("--seeding"))
    {
        await app.Services.RunSeeding();
    }*/
}

app.UseCors(policyBuilder =>
{
    policyBuilder.WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod();
});

app.UseSerilogRequestLogging();

app.UseExceptionMiddleware();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();

namespace DirectoryService.API
{
    public partial class Program;
}