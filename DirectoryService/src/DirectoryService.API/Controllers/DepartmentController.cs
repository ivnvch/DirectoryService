using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels.Departments;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;
using DirectoryService.Application.Departments.Queries.GetDescendantsDepartments;
using DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithPreloadingChildren;
using DirectoryService.Shared;
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

    [HttpPatch("{departmentId:guid}/locations")]
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

    [HttpPut("{departmentId:guid}/parent")]
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

    [HttpGet("top-positions")]
    public async Task<EndpointResult<GetTopDepartmentsDto>> GetTopDepartments(
        [FromServices] IQueryHandler<GetTopDepartmentsDto> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(cancellationToken);
    }

    [HttpGet("roots/")]
    public async Task<EndpointResult<PaginationResponse<GetRootDepartmentDto>>> GetRootDepartmentsWithPreloadingChildren(
        [FromQuery] GetRootDeparmentsRequest request,
        [FromServices] GetRootDepartmentsHandler handler,
        CancellationToken cancellationToken)
    {
        GetRootDepartmentsQuery query = new GetRootDepartmentsQuery(request);
        
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("{id:guid}/children")]
    public async Task<EndpointResult<List<GetDescendantsDepartmentDto>>> UploadingDescendants(
        [FromRoute] Guid id,
        [FromQuery] PaginationRequest pagination,
        [FromServices] GetDescendantsDepartmentHandler handler,
        CancellationToken cancellationToken)
    {
        GetDescendantsDepartmentQuery query = new GetDescendantsDepartmentQuery(id,  pagination);
        return await handler.HandleList(query, cancellationToken);
    }

    [HttpDelete("{departmentId}:guid")]
    public async Task<EndpointResult<Guid>> SoftDeleteDepartment(
        [FromRoute] Guid departmentId,
        [FromServices] SoftDeleteDepartmentHandler handler,
        CancellationToken cancellationToken)
    {
        SoftDeleteDepartmentCommand command = new SoftDeleteDepartmentCommand(departmentId);
        
        return await handler.Handle(command, cancellationToken);
    }
}