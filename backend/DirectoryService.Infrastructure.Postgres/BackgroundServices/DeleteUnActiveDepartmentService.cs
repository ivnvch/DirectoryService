using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class DeleteUnActiveDepartmentService : BackgroundService
{
    private readonly DeleteUnActiveDepartmentOptions _deleteUnActiveDepartmentOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DeleteUnActiveDepartmentService> _logger;

    public DeleteUnActiveDepartmentService(
        IOptions<DeleteUnActiveDepartmentOptions> deleteUnActiveDepartmentOptions,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DeleteUnActiveDepartmentService> logger)
    {
        _deleteUnActiveDepartmentOptions = deleteUnActiveDepartmentOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        if (_deleteUnActiveDepartmentOptions.RunAtUtc < TimeSpan.Zero ||
            _deleteUnActiveDepartmentOptions.RunAtUtc >= TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(deleteUnActiveDepartmentOptions),
                "RunAtUtc must be in range [00:00:00, 24:00:00).");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextRunUtc = GetNextRunUtc(DateTime.UtcNow, _deleteUnActiveDepartmentOptions.RunAtUtc);
                var delay = nextRunUtc - DateTime.UtcNow;

                _logger.LogInformation(
                    "Delete unactive department service scheduled. Next run at {NextRunUtc}",
                    nextRunUtc);

                await Task.Delay(delay, stoppingToken);
            
                _logger.LogInformation("Delete unactive department service was started.");
                
                await Delete(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Delete unactive department service was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during delete unactive department service");
            }
        }
    }

    private async Task Delete(CancellationToken cancellationToken)
    {

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        var deleteDepartmentService = scope.ServiceProvider.GetRequiredService<DeleteDepartmentService>();

        var dateTime = DateTime.UtcNow - _deleteUnActiveDepartmentOptions.RetentionPeriod;
        
        await deleteDepartmentService.DeleteDepartment(dateTime, cancellationToken);
       
    }

    private static DateTime GetNextRunUtc(DateTime nowUtc, TimeSpan runAtUtc)
    {
        var todayRunUtc = nowUtc.Date.Add(runAtUtc);
        return nowUtc < todayRunUtc
            ? todayRunUtc
            : todayRunUtc.AddDays(1);
    }
    
    
}