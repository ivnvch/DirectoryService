using DirectoryService.Shared.Locations;

namespace DirectoryService.API.Models.RequestModels.Locations;

public record CreateLocationRequest(string Name, AddressDto address, string Timezone);