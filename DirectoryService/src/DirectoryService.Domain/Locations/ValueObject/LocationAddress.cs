using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationAddress
{
    public LocationAddress(string value)
    {
        Value = value;
    }
    
    public string Value { get; private set; }

    public static Result<LocationAddress, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsInvalid("Address");
        
        return new LocationAddress(value);
    }
}