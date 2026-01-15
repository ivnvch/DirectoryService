using DirectoryService.API.EndpointResults;
using DirectoryService.API.Models.RequestModels.Departments;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Departments.Commands.CreateDepartments;
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
        CancellationToken cancellation)
    {
        CreateDepartmentCommand command = new CreateDepartmentCommand(
            request.Name,
            request.Identifier,
            request.ParentId,
            request.LocationIds.ToArray());
        return await handler.Handle(command, cancellation);
    }
}