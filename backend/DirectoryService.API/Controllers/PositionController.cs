using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels.Positions;
using Shared.Abstractions;
using DirectoryService.Application.Positions.Commands.CreatePositions;
using DirectoryService.Application.Positions.Commands.SoftDeletePosition;
using DirectoryService.Application.Positions.Queries.GetPositionDetails;
using DirectoryService.Application.Positions.Queries.GetPositions;
using DirectoryService.Shared;
using DirectoryService.Shared.Positions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionController : ControllerBase
{

    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreatePositionRequest request,
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        CreatePositionCommand command = new CreatePositionCommand(
            request.Name,
            request.Description,
            request.DepartmentIds);
        
        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PaginationResponse<GetPositionDto>>> Get(
        [FromQuery] GetPositionsRequest request,
        [FromServices] IQueryHandler<PaginationResponse<GetPositionDto>, GetPositionQuery> handler,
        CancellationToken cancellationToken)
    {
        GetPositionQuery query = new GetPositionQuery(
            request.DepartmentIds,
            request.Search,
            request.IsActive,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDirection);
        
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<EndpointResult<GetPositionDetailsDto>> Get(
        [FromRoute] Guid id,
        [FromServices] IQueryHandler<GetPositionDetailsDto, GetPositionDetailsQuery> hander,
        CancellationToken cancellationToken)
    {
        GetPositionDetailsQuery query = new GetPositionDetailsQuery(id);
        
        return await hander.Handle(query, cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public async Task<EndpointResult<Guid>> Delete(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<Guid, SoftDeletePositionCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeletePositionCommand(id);
        return await handler.Handle(command, cancellationToken);
    }
}