using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Repositories;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Positions.Errors;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DirectoryService.Infrastructure.Positions.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly DirectoryDbContext _context;
    private readonly ILogger<PositionRepository> _logger;

    public PositionRepository(DirectoryDbContext context, 
        ILogger<PositionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken)
    {
        _context.Positions.Add(position);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Position {position.Name} with Position ID: {position.Id} has been added");

            return position.Id;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx is { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: not null } &&
                pgEx.ConstraintName.Contains("idx_position_name_active", StringComparison.InvariantCultureIgnoreCase))
            {
                return PositionError.PositionNameConflict(position.Name);
            }
            
            _logger.LogError(ex, "Database udpate error while creating position: {position}", position.Name);

            return PositionError.DatabaseError();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while creating position: {position.Name}", position.Name);

            return PositionError.OperationCancelled();
        }
        catch (Exception ex)
        {
           _logger.LogError(ex, "Unexpected error while adding Position: {position.Name}", position.Name);

           return PositionError.DatabaseError();
        }
    }

    public async Task<Result<bool, Error>> ExistsActiveWithName(string name, CancellationToken cancellationToken)
    {
       return await _context.Positions
            .AsNoTracking()
            .AnyAsync(p => p.IsActive && p.Name == name, cancellationToken: cancellationToken);
    }
}