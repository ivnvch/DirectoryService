using CSharpFunctionalExtensions;
using DirectoryService.Application.CQRS;
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
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _createLocationValidator;

    public CreateLocationHandler(ILocationRepository locationRepository, ILogger<CreateLocationHandler> logger,
        IValidator<CreateLocationCommand> createLocationValidator)
    {
        _locationRepository = locationRepository;
        _logger = logger;
        _createLocationValidator = createLocationValidator;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _createLocationValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
          

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
        if(addedLocation.IsFailure)
            return addedLocation.Error.ToErrors();
        
        _logger.LogInformation($"Created location with id {location.Value.Id}");

        return location.Value.Id;
    }
}