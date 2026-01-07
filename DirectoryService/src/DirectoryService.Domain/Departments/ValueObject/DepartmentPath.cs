using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Departments.ValueObject;

public record DepartmentPath
{
    private DepartmentPath(string value)
    {
        Value = value;
    }
    
    public string Value { get; }

    public static Result<DepartmentPath> CreateParent(DepartmentIdentifier identifier)
    {
        return new DepartmentPath(IsValidDepartmentPath(identifier.Value).Value);
    }
    
    public Result<DepartmentPath> CreateChild(DepartmentIdentifier childIdentifier)
    {
        return new DepartmentPath(IsValidDepartmentPath(childIdentifier.Value, Value).Value);
    }
    
    public static Result<string, Error> IsValidDepartmentPath(string path, string? existingPath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return GeneralErrors.ValueIsInvalid("path");

        if (!PathRegex.IsMatch(path))
            return GeneralErrors.ValueIsInvalid("path");

        if (!string.IsNullOrWhiteSpace(existingPath))
        {
            if (!PathRegex.IsMatch(existingPath))
                return GeneralErrors.ValueIsInvalid("path");

            string prefix = existingPath + ".";
            if (!path.StartsWith(prefix, StringComparison.Ordinal))
                return GeneralErrors.ValueIsInvalid("path");
        }

        return path;
    }

    private static readonly Regex PathRegex = new Regex(
        @"^[A-Za-z][A-Za-z0-9_-]*(\.[A-Za-z][A-Za-z0-9_-]*)*$",
        RegexOptions.Compiled);

}