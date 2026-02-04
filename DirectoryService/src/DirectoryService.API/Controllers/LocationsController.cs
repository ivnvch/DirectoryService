using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Locations.Commands.CreateLocations;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Shared.Locations.ResponseModels;
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
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request.Name, request.address, request.Timezone);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<GetLocationsDto>> Get(
        [FromQuery]  GetLocationsRequest request, 
        [FromServices] IQueryHandler<GetLocationsDto, GetLocationsQuery> query,
        CancellationToken cancellationToken)
    {
        var locationQuery = new GetLocationsQuery(
            request.DepartmentIds,
            request.Search,
            request.IsActive,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDirection);
        
        return await query.Handle(locationQuery, cancellationToken);
    }
}