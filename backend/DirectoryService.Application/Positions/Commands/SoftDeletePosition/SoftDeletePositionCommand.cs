using Shared.Abstractions;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePosition;

public record SoftDeletePositionCommand(Guid PositionId) : ICommand;
