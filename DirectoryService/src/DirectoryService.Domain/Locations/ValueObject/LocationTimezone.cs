using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using NodaTime;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationTimezone
{
    public LocationTimezone(string value)
    {
        Value = value;
    }
    
    public string Value { get; }
    public static Result<LocationTimezone, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("Timezone");
        
        var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(value);
        if (zone is null)
            return  GeneralErrors.ValueIsInvalid("Timezone");
        
        return new LocationTimezone(value);
    }
}