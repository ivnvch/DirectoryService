using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;

public record UpdateDepartmentPathCommand(Guid DepartmentId, Guid? ParentId) : ICommand;