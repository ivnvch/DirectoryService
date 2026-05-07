using Shared.CommonErrors;
using FluentValidation;
using Shared.Validation;

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