using System.Text.Json;
using DirectoryService.Shared.Errors;
using FluentValidation.Results;

namespace DirectoryService.Application.Extensions.Validation;

public static class ValidationExtensions
{
    public static Error ToError(this ValidationResult validationResult)
    {
        List<ValidationFailure> validationErrors = validationResult.Errors;

        IEnumerable<IReadOnlyList<ErrorMessage>> errors = from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = TryDeserializeError(errorMessage, out var deserialized)
                ? deserialized.Messages
                : new[]
                {
                    new ErrorMessage(
                        validationError.ErrorCode,
                        validationError.ErrorMessage,
                        validationError.PropertyName)
                }
            select error;
        
        return Error.Validation(errors.SelectMany(error => error));
    }

    private static bool TryDeserializeError(string errorMessage, out Error error)
    {
        error = null!;

        if (string.IsNullOrWhiteSpace(errorMessage))
            return false;

        if (errorMessage[0] != '{')
            return false;

        try
        {
            error = JsonSerializer.Deserialize<Error>(errorMessage)!;
            return error is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}