using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Departments.Errors;

public static class DepartmentError
{
    public static Error DatabaseError() =>
        Error.Failure(new ErrorMessage("department.database.error", "Ошибка при работе с подразделением"));
    
    public static Error OperationCancelled() =>
        Error.Failure(new ErrorMessage("department.operation.cancelled", "Операция была отменена"));
}