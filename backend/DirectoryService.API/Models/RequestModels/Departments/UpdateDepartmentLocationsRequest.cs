using Shared.Abstractions;

namespace DirectoryService.API.Models.RequestModels.Departments;

public record UpdateDepartmentLocationsRequest(IReadOnlyCollection<Guid> LocationIds);