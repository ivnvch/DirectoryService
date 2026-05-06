using Shared.CommonErrors;
using FluentValidation;
using Shared.Validation;

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