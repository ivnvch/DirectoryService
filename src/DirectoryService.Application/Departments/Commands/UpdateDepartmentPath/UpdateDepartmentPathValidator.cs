using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;

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