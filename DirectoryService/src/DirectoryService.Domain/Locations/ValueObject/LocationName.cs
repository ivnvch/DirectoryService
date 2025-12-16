using CSharpFunctionalExtensions;
using DirectoryService.Shared.Constants;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationName
{
    private LocationName(string name)
    {
        Value = name;
    }
    
    public string Value { get; private set; }

    public static Result<LocationName> Create(string name)
    {
        if(string.IsNullOrWhiteSpace(name) && (name.Length > LengthConstant.Min2Length || name.Length < LengthConstant.Max150Length))
            return  Result.Failure<LocationName>("Name cannot be empty");
        
        return new LocationName(name);
    }
    
}