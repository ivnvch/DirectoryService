using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels.Positions;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Positions.Commands.CreatePositions;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}