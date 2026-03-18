using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Locations.Commands.DeleteLocation;

public record DeleteLocationCommand(Guid LocationId) : ICommand;
