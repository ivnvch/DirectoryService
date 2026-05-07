using CSharpFunctionalExtensions;
using DirectoryService.Shared.Constants;
using Shared.CommonErrors;
using DirectoryService.Shared.Validations;

namespace DirectoryService.Domain.Departments.ValueObject;

public record DepartmentIdentifier
{
    private DepartmentIdentifier(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentIdentifier, Error> Create(string value)
    {
        if((value.Length < LengthConstant.Min2Length || value.Length > LengthConstant.Max150Length) 
           && !CheckLatinLetters.OnlyLatinLetters(value))
            return GeneralErrors.ValueIsInvalid($"'{value}'");

        return new DepartmentIdentifier(value);
    }
}