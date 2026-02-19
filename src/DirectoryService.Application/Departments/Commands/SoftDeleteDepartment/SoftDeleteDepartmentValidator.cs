using System.Runtime.InteropServices.JavaScript;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public class SoftDeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull()
            .WithError(Error.Validation("departmentId.is.null", "DepartmentId cannot be null."));
    }
}