

using Shared.CommonErrors;

namespace DirectoryService.Domain.Positions.Errors;

public static class PositionError
{
    public static Error OperationCancelled() =>
        Error.Failure(new ErrorMessage("position.operation.cancelled",
            "Operation was cancelled"));
    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage("position.database.error", 
            "The error occurred while working with the database"));
    
    public static Error PositionNameConflict(string positionName) =>
        Error.Conflict("position.conflict.name_conflict",
            $"Position with this name: {positionName} already exists");

    public static Error NotFound(Guid id) =>
        Error.NotFound("position.not.found", $"Позиция с id {id} не найдена.");
}