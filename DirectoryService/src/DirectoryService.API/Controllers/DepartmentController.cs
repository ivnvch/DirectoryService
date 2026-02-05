using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels.Departments;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.API.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentController : ControllerBase
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        CreateDepartmentCommand command = new CreateDepartmentCommand(
            request.Name,
            request.Identifier,
            request.ParentId,
            request.LocationIds.ToArray());
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPatch("/{departmentId:guid}/locations")]
    public async Task<EndpointResult<Guid>> UpdateLocations(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentLocationCommand>  handler,
        [FromBody] UpdateDepartmentLocationsRequest request,
        CancellationToken cancellationToken)
    {
        UpdateDepartmentLocationCommand command = new UpdateDepartmentLocationCommand(
            departmentId,
            request.LocationIds);
        
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut("/{departmentId:guid}/parent")]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(400)]
    public async Task<EndpointResult<Guid>> UpdateDepartmentPath(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, UpdateDepartmentPathCommand>  handler,
        [FromBody] Guid? parentId,
        CancellationToken cancellationToken)
    {
        UpdateDepartmentPathCommand pathCommand = new UpdateDepartmentPathCommand(departmentId, parentId);
        return await handler.Handle(pathCommand, cancellationToken);
    }

    [HttpGet("/top-positions")]
    public async Task<EndpointResult<GetTopDepartmentsDto>> GetTopDepartments(
        [FromServices] IQueryHandler<GetTopDepartmentsDto> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }
}