using FluentValidation;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .MaximumLength(150)
            .WithMessage("Name length must be between 3 and 150 characters");
        
        RuleFor(x => x.Address.Country)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100)
            .WithMessage("Country length must be less than 100 characters");
        
        RuleFor(x => x.Address.City)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100)
            .WithMessage("City length must be less than 100 characters");
        
        RuleFor(x => x.Address.Street)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100)
            .WithMessage("Street length must be less than 100 characters");
        
        RuleFor(x => x.Address.House)
            .NotEmpty()
            .NotNull()
            .MaximumLength(100)
            .WithMessage("House length must be less than 100 characters");
        
        RuleFor(x => x.Address.Apartment)
            .MaximumLength(100)
            .WithMessage("Apartment length must be less than 100 characters");
        
        RuleFor(x => x.Timezone)
            .NotNull()
            .NotEmpty()
            .Must(t => TimeZoneInfo.TryFindSystemTimeZoneById(t, out _))
            .WithMessage("Timezone must be valid IANA code");;
            
    }
}