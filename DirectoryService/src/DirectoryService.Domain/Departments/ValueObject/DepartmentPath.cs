using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments.ValueObject;

public record DepartmentPath
{
    private DepartmentPath(string value)
    {
        Value = value;
    }
    
    public string Value { get; init; }

    public static Result<DepartmentPath> Create(string value, string? existingPath = null)
    {
        return new DepartmentPath(IsValidDepartmentPath(value, existingPath).Value);
    }
    
    public static Result<string> IsValidDepartmentPath(string path, string? existingPath = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Result.Failure<string>("Invalid path");

        if (!PathRegex.IsMatch(path))
            return Result.Failure<string>("Invalid path");

        if (!string.IsNullOrWhiteSpace(existingPath))
        {
            if (!PathRegex.IsMatch(existingPath))
                return Result.Failure<string>("Invalid path");

            string prefix = existingPath + ".";
            if (!path.StartsWith(prefix, StringComparison.Ordinal))
                return Result.Failure<string>("Invalid path");
        }

        return path;
    }

    private static readonly Regex PathRegex = new Regex(
        @"^[A-Za-z][A-Za-z0-9_-]*(\.[A-Za-z][A-Za-z0-9_-]*)*$",
        RegexOptions.Compiled);

}