using CSharpFunctionalExtensions;
using DirectoryService.Shared.Constants;
using DirectoryService.Shared.Validations;

namespace DirectoryService.Domain.Departments.ValueObject;

public record DepartmentIdentifier
{
    private DepartmentIdentifier(string value)
    {
        Value = value;
    }
    
    public string Value { get; init; }

    public static Result<DepartmentIdentifier> Create(string value)
    {
        if((value.Length < LengthConstant.Min2Length || value.Length > LengthConstant.Max150Length) 
           && !CheckLatinLetters.OnlyLatinLetters(value))
            return Result.Failure<DepartmentIdentifier>($"The value '{value}' is invalid");

        return new DepartmentIdentifier(value);
    }
}