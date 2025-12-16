using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationAddress
{
    public LocationAddress(string value)
    {
        Value = value;
    }
    
    public string Value { get; private set; }

    public static Result<LocationAddress> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return Result.Failure<LocationAddress>("Address cannot be empty");
        
        return new LocationAddress(value);
    }
}