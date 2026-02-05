using DirectoryService.Shared.Locations;

namespace DirectoryService.API.Models.RequestModels;

public record CreateLocationRequest(string Name, AddressDto address, string Timezone);