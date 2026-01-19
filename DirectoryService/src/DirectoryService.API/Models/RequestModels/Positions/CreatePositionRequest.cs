namespace DirectoryService.API.Models.RequestModels.Positions;

public record CreatePositionRequest(string Name, string? Description, List<Guid> DepartmentIds);