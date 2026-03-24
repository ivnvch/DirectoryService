namespace Shared.Errors;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} недействительно", name);
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}", null);
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name == null ? string.Empty : " " + name + " ";
        return Error.Validation("length.is.invalid", $"Поле{label}обязательно", null);
    }

    public static Error AlreadyExists()
    {
        return Error.Conflict("record.already.exist", "Запись уже существует", null);
    }
    
    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Серверная ошибка", null);
    }
    
    public static Error Forbidden(string message = "The server denied access to the requested resource due to insufficient permissions.") 
        => Error.Conflict("forbidden", message);
}