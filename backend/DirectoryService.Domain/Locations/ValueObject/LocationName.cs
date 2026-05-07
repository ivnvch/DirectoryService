using CSharpFunctionalExtensions;
using DirectoryService.Shared.Constants;
using Shared.CommonErrors;

namespace DirectoryService.Domain.Locations.ValueObject;

public record LocationName
{
    private LocationName(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<LocationName, Error> Create(string name)
    {
        if(string.IsNullOrWhiteSpace(name) && (name.Length is > LengthConstant.Min2Length or < LengthConstant.Max150Length))
            return  GeneralErrors.ValueIsInvalid("Name cannot be empty");
        
        return new LocationName(name);
    }
    
}