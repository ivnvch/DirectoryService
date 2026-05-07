using Shared.Abstractions;

namespace DirectoryService.Application.Locations.Commands.DeleteLocation;

public record DeleteLocationCommand(Guid LocationId) : ICommand;
