using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;

public record UpdateDepartmentLocationCommand(Guid DepartmentId, IEnumerable<Guid> LocationIds) : ICommand;