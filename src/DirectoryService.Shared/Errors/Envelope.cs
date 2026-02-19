using System.Text.Json.Serialization;

namespace DirectoryService.Shared.Errors;

public record Envelope
{
    public object? Result { get; }

    public Errors? ErrorList { get; }

    public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());
    
    public DateTime TimeGenerated { get; }
    
    [JsonConstructor]
    private Envelope(object? result, Errors? errorList)
    {
        Result = result;
        ErrorList = errorList;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok(object? result = null) =>
        new(result, null);
    
    public static Envelope Error(Errors errors) => 
        new(null, errors);

}

public record Envelope<T>
{
    public T? Result { get; }
    
    public Errors? ErrorsList { get; }
    
    public bool IsError => ErrorsList != null || (ErrorsList != null && ErrorsList.Any());
    public DateTime TimeGenerated { get; }
    
    [JsonConstructor]
    public Envelope(T? result, Errors? errorsList)
    {
        Result = result;
        ErrorsList = errorsList;
        TimeGenerated = DateTime.Now;
    }

    public static Envelope<T> Ok(T? result = default) => new(result, null);

    public static Envelope<T> Error(Errors errorsList) => new(default, errorsList);
    
}