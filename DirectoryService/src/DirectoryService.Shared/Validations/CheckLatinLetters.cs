using System.Text.RegularExpressions;

namespace DirectoryService.Shared.Validations;

public static class CheckLatinLetters
{
    public static bool OnlyLatinLetters(string value)
    {
        return Regex.IsMatch(value, @"^[A-Za-z](?:[A-Za-z]|-(?!-))*$");
    }
}