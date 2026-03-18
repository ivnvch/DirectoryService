using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePosition;

public record SoftDeletePositionCommand(Guid PositionId) : ICommand;
