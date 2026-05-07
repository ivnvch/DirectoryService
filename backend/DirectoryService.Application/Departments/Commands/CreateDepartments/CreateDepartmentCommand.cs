using Shared.Abstractions;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public record CreateDepartmentCommand(string Name, string Identifier, Guid? ParentId, Guid[] LocationIds) : ICommand;