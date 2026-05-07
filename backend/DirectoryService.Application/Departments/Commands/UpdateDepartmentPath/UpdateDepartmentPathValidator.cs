using DirectoryService.Application.Departments.Repositories;
using Shared.CommonErrors;
using FluentValidation;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;

public class UpdateDepartmentPathValidator : AbstractValidator<UpdateDepartmentPathCommand>
{
    public UpdateDepartmentPathValidator(IDepartmentRepository departmentRepository)
    {
        RuleFor(x => x.DepartmentId)
            .NotNull()
            .NotEmpty()
            .WithError(Error.Validation("department.id.is.null", "DepartmentId must not be empty."));
    }
}