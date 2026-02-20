using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirectoryService.Shared.Errors;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);
public record Error
{
    private const string SEPARATOR = "||";
    public IReadOnlyList<ErrorMessage> Messages { get; } = [];
    public ErrorType Type { get; }

    [JsonConstructor]
    private Error(IReadOnlyList<ErrorMessage> messages,ErrorType type)
    {
        Messages = messages.ToArray();
        Type = type;
    }
    
    private Error(IEnumerable<ErrorMessage> messages,ErrorType type)
    {
        Messages = messages.ToArray();
        Type = type;
    }
    
    public static Error Validation(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.VALIDATION);
    public static Error NotFound(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.NOT_FOUND);
    public static Error Failure(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.FAILURE);
    public static Error Conflict(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.CONFLICT);
    public static Error Authentication(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.AUTHENTICATION);
    public static Error Authorization(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.AUTHORIZATION);
    
    public static Error Validation(params IEnumerable<ErrorMessage> messages)
        => new(messages, ErrorType.VALIDATION);
    public static Error NotFound(params IEnumerable<ErrorMessage> messages)   
        => new(messages, ErrorType.NOT_FOUND);
    public static Error Failure(params IEnumerable<ErrorMessage> messages)   
        => new(messages, ErrorType.FAILURE);
    public static Error Conflict(params IEnumerable<ErrorMessage> messages)   
        => new(messages, ErrorType.CONFLICT);
    public static Error Authentication(params IEnumerable<ErrorMessage> messages)   
        => new(messages, ErrorType.AUTHENTICATION);
    public static Error Authorization(params IEnumerable<ErrorMessage> messages)   
        => new(messages, ErrorType.AUTHORIZATION);
    
    public Errors ToErrors()
        => new([this]);

    public string Serialize()//правильно ли?
    {
        string serialize = "";
        foreach (var message in Messages)
        {
           serialize = string.Join(SEPARATOR, message.Code, message.Message, Type);
        }
        
        return serialize;
    }

    /*public static Error Deserialize(string serialized)//правильно ли?
    {
        
        
        string[] parts = serialized.Split(SEPARATOR);
        if (parts.Length < 3)
        {
            throw new ArgumentException($"Invalid format: {serialized}");
        }

        if (Enum.TryParse<ErrorType>(parts[2], out var type) == false)
        {
            throw new ArgumentException($"Invalid format: {serialized}");
        }
        
        return new Error(new ErrorMessage[]{new ErrorMessage(parts[0], parts[0])}, type);
    }*/
    
    public static Error Deserialize(string json)
        => JsonSerializer.Deserialize<Error>(json)
           ?? throw new ArgumentException("Invalid Error json");
    
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    FAILURE,
    CONFLICT,
    AUTHENTICATION,
    AUTHORIZATION
}