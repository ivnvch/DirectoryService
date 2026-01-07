using CSharpFunctionalExtensions;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.Shared.Errors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _createLocationValidator;

    public CreateLocationHandler(ILocationRepository locationRepository, ILogger<CreateLocationHandler> logger, IValidator<CreateLocationCommand> createLocationValidator)
    {
        _locationRepository = locationRepository;
        _logger = logger;
        _createLocationValidator = createLocationValidator;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _createLocationValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return GeneralErrors.ValueIsInvalid("location").ToErrors();
        
        var name = LocationName.Create(request.Name);
        
        var address = LocationAddress.Create(
            request.Address.Country,
            request.Address.City,
            request.Address.Street,
            request.Address.House,
            request.Address.Apartment);
        
        var timezone = LocationTimezone.Create(request.Timezone);
        
        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value);
        
        if(location.IsFailure)
            return GeneralErrors.ValueIsInvalid("location").ToErrors();
        
       await _locationRepository.AddAsync(location.Value, cancellationToken);
       _logger.LogInformation($"Created location with id {location.Value.Id}");

        return location.Value.Id;
    }
    
}