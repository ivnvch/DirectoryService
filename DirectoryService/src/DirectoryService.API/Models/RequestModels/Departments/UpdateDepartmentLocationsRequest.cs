using DirectoryService.Application.CQRS;

namespace DirectoryService.API.Models.RequestModels.Departments;

public record UpdateDepartmentLocationsRequest(IReadOnlyCollection<Guid> LocationIds);