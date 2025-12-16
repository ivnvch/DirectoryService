using CSharpFunctionalExtensions;
using DirectoryService.Shared.Constants;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationName
{
    private LocationName(string value)
    {
        Value = value;
    }
    
    public string Value { get; private set; }

    public static Result<LocationName, Error> Create(string name)
    {
        if(string.IsNullOrWhiteSpace(name) && (name.Length > LengthConstant.Min2Length || name.Length < LengthConstant.Max150Length))
            return  GeneralErrors.ValueIsInvalid("Name cannot be empty");
        
        return new LocationName(name);
    }
    
}