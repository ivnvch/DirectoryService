using DirectoryService.Application.CQRS;
using DirectoryService.Shared.Locations;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public record CreateLocationCommand(string Name, AddressDto Address, string Timezone) : ICommand;