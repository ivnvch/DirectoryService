using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Positions.Commands.CreatePositions;

public record CreatePositionCommand(string Name, string? Description, List<Guid> DepartmentIds) : ICommand;