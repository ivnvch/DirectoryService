using DirectoryService.Application.CQRS;
using DirectoryService.Shared.Locations;

namespace DirectoryService.Application.Locations.Commands.UpdateLocation;

public record UpdateLocationCommand(Guid LocationId, string Name, AddressDto Address, string Timezone) : ICommand;