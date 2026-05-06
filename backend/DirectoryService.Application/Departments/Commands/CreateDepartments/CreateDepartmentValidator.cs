using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Domain.Departments.ValueObject;
using Shared.CommonErrors;
using FluentValidation;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator(ILocationRepository locationRepository)
    {
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
    }
}