using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments.Repositories;
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
        var departmentRepository = scope.ServiceProvider.GetRequiredService<IDepartmentRepository>();
        var transactionManager = scope.ServiceProvider.GetRequiredService<ITransactionManager>();

        var dateTime = DateTime.UtcNow - _deleteUnActiveDepartmentOptions.RetentionPeriod;
        
        var departmentIds = await departmentRepository.GetIdsForDeleteAsync(dateTime, cancellationToken);
        
        if (departmentIds.Count == 0)
        {
            _logger.LogInformation("No departments found for delete");
            return;
        }

        foreach (var departmentId in departmentIds)
        {
            var transactionScopeResult = await transactionManager.BeginTransactionAsync(cancellationToken);
            
            if (transactionScopeResult.IsFailure)
            {
                _logger.LogError("Failed to start transaction for delete, departmentId: {DepartmentId}", departmentId);
                continue;
            }

            using var transactionScope = transactionScopeResult.Value;

            var updatePathInHierarchyBeforeDelete =
                await departmentRepository.UpdateChildrenHierarchyBeforeDeleteAsync(departmentId, cancellationToken);

            if (updatePathInHierarchyBeforeDelete.IsFailure)
            {
                transactionScope.Rollback();
                
                _logger.LogError("Failed to update hierarchy before delete, departmentId: {DepartmentId}", departmentId);
                continue;
            }

            var deleteDepartment = await departmentRepository.DeleteByIdAsync(departmentId, cancellationToken);

            if (deleteDepartment.IsFailure)
            {
                transactionScope.Rollback();
                _logger.LogError("Failed to delete department, departmentId: {DepartmentId}", departmentId);
                continue;
            }
            
            var saveChanges = await transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChanges.IsFailure)
            {
                transactionScope.Rollback();
                _logger.LogError("Failed to save changes during delete, departmentId: {DepartmentId}", departmentId);
                continue;
            }

            var committedResult = transactionScope.Commit();
            if (committedResult.IsFailure)
            {
                _logger.LogError("Failed to commit transaction during delete, departmentId: {DepartmentId}", departmentId);
                continue;
            }

            _logger.LogInformation("Department deleted successfully, departmentId: {DepartmentId}", departmentId);
        }
       
        _logger.LogInformation("Delete completed, processed departments count: {Count}", departmentIds.Count);


    }

    private static DateTime GetNextRunUtc(DateTime nowUtc, TimeSpan runAtUtc)
    {
        var todayRunUtc = nowUtc.Date.Add(runAtUtc);
        return nowUtc < todayRunUtc
            ? todayRunUtc
            : todayRunUtc.AddDays(1);
    }
    
    
}