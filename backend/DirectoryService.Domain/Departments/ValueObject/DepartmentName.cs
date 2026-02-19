using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Departments.ValueObject;

public record DepartmentName
{
    public const int NAME_MIN_LENGTH = 3;
    public const int NAME_MAX_LENGTH = 150;
    
    public DepartmentName(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if(string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("department name");
        
        if(value.Length is <  NAME_MIN_LENGTH or > NAME_MAX_LENGTH)
            return GeneralErrors.ValueIsInvalid("department name length");
        
        return new DepartmentName(value);
    }
}