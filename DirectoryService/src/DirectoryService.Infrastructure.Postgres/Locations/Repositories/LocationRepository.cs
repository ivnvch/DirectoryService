using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.Errors;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Locations.Repositories;

public class LocationRepository :  ILocationRepository
{
    private readonly DirectoryDbContext _context;
    private readonly ILogger<LocationRepository> _logger;

    public LocationRepository(DirectoryDbContext context, ILogger<LocationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellation = default)
    {
             _context.Locations.Add(location);
        try
        {

            await _context.SaveChangesAsync(cancellation);

            _logger.LogInformation($"Location {location.Id} has been added.");

            return location.Id;
        }
        /*catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("location", StringComparison.InvariantCultureIgnoreCase))
            {

            }

            _logger.LogError(ex, "Database update error while creating location: {location}", location);

            return LocationErrors.DatabaseError();
        }*/
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while creating location: {location}", location);

            return LocationErrors.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating location: {location}", location);
            return LocationErrors.DatabaseError();
        }
    }

    public async Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> locationIds, CancellationToken cancellation = default)
    {
        if (locationIds.Count == 0)
            return true;
        
        var existingCount = await _context.Locations
            .Where(l => locationIds.Contains(l.Id))
            .CountAsync(cancellation);
        
        return existingCount == locationIds.Count;
        
        
    }
}