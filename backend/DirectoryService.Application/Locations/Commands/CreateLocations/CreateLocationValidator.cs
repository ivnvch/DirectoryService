using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations.ValueObject;
using Shared.Errors;
using DirectoryService.Shared.Locations;
using FluentValidation;
using Shared.Validation;

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

        RuleFor(x =>  x.Address)
            .MustBeValueObject( LocationAddressFactory.FromDto);

        RuleFor(x => x.Timezone)
            .MustBeValueObject(LocationTimezone.Create);
    }
}

public static class LocationAddressFactory
{
    public static Result<LocationAddress, Error> FromDto(AddressDto? dto)
    {
        if (dto is null)
            return GeneralErrors.ValueIsInvalid("address");

        return LocationAddress.Create(
            dto.Country,
            dto.City,
            dto.Street,
            dto.House,
            dto?.Apartment ?? null
        );
    }
}