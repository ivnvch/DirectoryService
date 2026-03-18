using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels;
using DirectoryService.API.Models.RequestModels.Locations;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Locations.Commands.CreateLocations;
using DirectoryService.Application.Locations.Commands.DeleteLocation;
using DirectoryService.Application.Locations.Commands.UpdateLocation;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Shared;
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
        var command = new CreateLocationCommand(request.Name, request.AddressDto, request.Timezone);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PaginationResponse<GetLocationDto>>> Get(
        [FromQuery]  GetLocationsRequest request, 
        [FromServices] IQueryHandler<PaginationResponse<GetLocationDto>, GetLocationsQuery> query,
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

    [HttpPut("{locationId:guid}")]
    public async Task<EndpointResult<Guid>> Update(
        [FromRoute] Guid locationId,
        [FromServices] ICommandHandler<Guid, UpdateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(
            locationId,
            request.Name,
            request.AddressDto,
            request.Timezone);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpDelete("{locationId:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid locationId,
        [FromServices] ICommandHandler<Guid, DeleteLocationCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLocationCommand(locationId);
        return await handler.Handle(command, cancellationToken);
    }
}