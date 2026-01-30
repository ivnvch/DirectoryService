using System.ComponentModel.Design.Serialization;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;

public class UpdateDepartmentLocationValidator : AbstractValidator<UpdateDepartmentLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    
    public UpdateDepartmentLocationValidator(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
        
        RuleFor(x => x.LocationIds)
            .NotNull()
            .NotEmpty()
            .MustAsync(async (ids, cancellationToken) =>
            {
                var result = await _locationRepository.AllExistAsync(ids.ToArray(), cancellationToken);
                return result is { IsSuccess: true, Value: true };
            })
            .WithError(Error.Validation(
                new ErrorMessage("locationIds.not.exists", "One or more locations are not exists")));
    }
}