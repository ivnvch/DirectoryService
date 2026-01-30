using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;

public class UpdateDepartmentPathValidator : AbstractValidator<UpdateDepartmentPathCommand>
{
    private readonly IDepartmentRepository _departmentRepository;
    public UpdateDepartmentPathValidator(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;

        RuleFor(x => x.DepartmentId)
            .NotNull()
            .NotEmpty()
            .WithError(Error.Validation("department.id.is.null", "DepartmentId must not be empty."));

        RuleFor(x => x)
            .Must(x => x.ParentId is null || x.ParentId != x.DepartmentId)
            .WithError(Error.Validation("parent.id.is.the.same.as.department.id", "ParentId must not be the same as DepartmentId."));
    }
}