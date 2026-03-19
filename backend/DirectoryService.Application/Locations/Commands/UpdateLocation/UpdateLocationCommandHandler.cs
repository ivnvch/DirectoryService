using CSharpFunctionalExtensions;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using Shared.Errors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public class UpdateLocationCommandHandler : ICommandHandler<Guid, UpdateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<UpdateLocationCommandHandler> _logger;

    public UpdateLocationCommandHandler(
        ILocationRepository locationRepository,
        ILogger<UpdateLocationCommandHandler> logger)
    {
        _locationRepository = locationRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var locationResult = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
            return locationResult.Error.ToErrors();

        var location = locationResult.Value;

        var nameResult = LocationName.Create(command.Name);
        if (nameResult.IsFailure)
            return nameResult.Error.ToErrors();

        var addressResult = LocationAddress.Create(
            command.Address.Country,
            command.Address.City,
            command.Address.Street,
            command.Address.House,
            command.Address.Apartment);
        if (addressResult.IsFailure)
            return addressResult.Error.ToErrors();

        var timezoneResult = LocationTimezone.Create(command.Timezone);
        if (timezoneResult.IsFailure)
            return timezoneResult.Error.ToErrors();

        var updateResult = location.Update(nameResult.Value, addressResult.Value, timezoneResult.Value);
        if (updateResult.IsFailure)
            return updateResult.Error.ToErrors();

        var saveResult = await _locationRepository.UpdateAsync(location, cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        _logger.LogInformation("Location {LocationId} has been updated.", command.LocationId);
        return command.LocationId;
    }
}
