using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;

public record UpdateDepartmentPathCommand(Guid DepartmentId, Guid? ParentId) : ICommand;