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
            let error = JsonSerializer.Deserialize<Error>(errorMessage)
            select error.Messages;
        
        return Error.Validation(errors.SelectMany(error => error));
                    
        /*IEnumerable<ErrorMessage> messages = validationResult.Errors.Select(v =>
            new ErrorMessage(
                v.ErrorCode,
                v.ErrorMessage,
                v.PropertyName));*/

        //return Error.Validation(messages).ToErrors();
    }
}