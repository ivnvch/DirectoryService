using Shared.CommonErrors;
using FluentValidation;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;

public class UpdateDepartmentLocationValidator : AbstractValidator<UpdateDepartmentLocationCommand>
{
    public UpdateDepartmentLocationValidator()
    {
        RuleFor(x => x.LocationIds)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));

        RuleFor(x => x.LocationIds)
            .Must(list => list.Any())
            .WithError(GeneralErrors.ValueIsRequired("locationIds"));
    }
}