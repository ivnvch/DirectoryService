using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Departments.MoveDepartments;

public record UpdateDepartmentPathCommand(Guid DepartmentId, Guid? ParentId) : ICommand;