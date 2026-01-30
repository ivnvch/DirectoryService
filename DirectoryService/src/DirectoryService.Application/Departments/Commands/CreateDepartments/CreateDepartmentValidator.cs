using System.ComponentModel.Design.Serialization;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    private readonly ILocationRepository _locationRepository;
    
    public CreateDepartmentValidator(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
        
        RuleFor(x => x.Name)
            .MustBeValueObject(DepartmentName.Create);
        
        RuleFor(x => x.Identifier)
            .MustBeValueObject(DepartmentIdentifier.Create);
        
        RuleFor(x => x.ParentId)
            .Must(id => id is null || id.Value != Guid.Empty)
            .WithError(GeneralErrors.ValueIsInvalid("parentId"));
        
        RuleFor(x => x.LocationIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));

        RuleFor(x => x.LocationIds)
            .Must(list => list.Length > 0)
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));

        RuleFor(x => x.LocationIds)
            .MustAsync(async (ids, cancellationToken) =>
            {
                if (ids is null)
                    return true;

               var result = await _locationRepository.AllExistAsync(ids, cancellationToken);
               
               return result is { IsSuccess: true, Value: true };
            })
            .WithError(Error.Validation(
                new ErrorMessage("CreateDepartmentCommand.LocationIds.is.not.unique", 
                              "Some LocationIds are not exists")));// нужно ли здесь пробрасывать ошибку
    }
}