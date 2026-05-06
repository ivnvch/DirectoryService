using Shared.CommonErrors;
using FluentValidation;
using Shared.Validation;

namespace DirectoryService.Application.Positions.Queries.GetPositions;

public class GetPositionValidator : AbstractValidator<GetPositionQuery>
{
    public GetPositionValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(1000)
            .WithError(GeneralErrors.ValueIsInvalid("search"));
        
        RuleFor(x => x.Page)
            .NotNull()
            .WithError(GeneralErrors.ValueIsInvalid("Page"));
        
        RuleFor(x => x.PageSize)
            .NotNull()
            .GreaterThan(0)
            .WithError(GeneralErrors.ValueIsInvalid("PageSize"));
    }
}