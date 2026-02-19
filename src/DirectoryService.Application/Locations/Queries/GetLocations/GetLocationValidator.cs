using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationValidator()
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