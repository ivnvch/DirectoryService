using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Locations.Errors;

public static class LocationErrors
{
    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage("location.database.error", "Ошибка БД при работе с локацией"));

    public static Error OperationCancelled() =>
         Error.Failure(new ErrorMessage("location.operation.cancelled", "Операция была отменена"));
}