using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.Locations;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager  _transactionManager;
    private readonly IValidator<CreateLocationCommand> _createLocationValidator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationRepository locationRepository, 
        ITransactionManager transactionManager,
        IValidator<CreateLocationCommand> createLocationValidator, 
        ILogger<CreateLocationHandler> logger)
    {
        _locationRepository = locationRepository;
        _logger = logger;
        _createLocationValidator = createLocationValidator;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _createLocationValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
          
        Result<ITransactionScope, Error> transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        ITransactionScope transactionScope = transactionScopeResult.Value;

        var name = LocationName.Create(command.Name).Value;

        var address = LocationAddress.Create(
            command.Address.Country,
            command.Address.City,
            command.Address.Street,
            command.Address.House,
            command.Address.Apartment)
        .Value;

        var timezone = LocationTimezone.Create(command.Timezone).Value;

        var location = Location.Create(
            name,
            address,
            timezone);

        if (location.IsFailure)
            return location.Error.ToErrors();

        var addedLocation = await _locationRepository.Add(location.Value, cancellationToken);
        if (addedLocation.IsFailure)
        {
            transactionScope.Rollback();
            return addedLocation.Error.ToErrors();
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();
        
        _logger.LogInformation($"Created location with id {location.Value.Id}");

        return location.Value.Id;
    }
}