using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Departments.Queries.GetDescendantsDepartments;

public class GetDescendantsDepartmentValidator : AbstractValidator<GetDescendantsDepartmentQuery>
{
    public GetDescendantsDepartmentValidator()
    {
        RuleFor(query => query.Id)
            .NotNull()
            .WithError(GeneralErrors.ValueIsRequired("Department ID"));
    }
}