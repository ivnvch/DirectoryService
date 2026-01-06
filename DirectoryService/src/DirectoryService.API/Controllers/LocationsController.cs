using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Locations.Commands.CreateLocations;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellation)
    {
        var command = new CreateLocationCommand(request.Name, request.address, request.Timezone);
        return await handler.Handle(command, cancellation);
    }
}