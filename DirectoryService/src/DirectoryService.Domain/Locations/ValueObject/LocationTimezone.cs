using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationTimezone
{
    public LocationTimezone(string value)
    {
        Value = value;
    }
    
    public string Value { get; private set; }
    public static Result<LocationTimezone> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationTimezone>("Timezone cannot be empty");
        
        return new LocationTimezone(value);
    }
}