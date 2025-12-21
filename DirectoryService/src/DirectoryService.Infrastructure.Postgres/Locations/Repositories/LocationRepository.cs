using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Locations;
using Microsoft.Extensions.Logging;

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

    public async Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellation = default)
    {
        try
        {
           await _context.Locations.AddAsync(location, cancellation);
           
           await _context.SaveChangesAsync(cancellation);
           
           _logger.LogInformation($"Location {location.Id} has been added.");

           return location.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return Result.Failure<Guid>(ex.Message);
        }
    }
}