using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;

public record SoftDeleteDepartmentCommand(Guid DepartmentId) : ICommand;