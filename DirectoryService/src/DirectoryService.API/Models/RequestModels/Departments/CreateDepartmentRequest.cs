namespace DirectoryService.API.Models.RequestModels.Departments;

public record CreateDepartmentRequest(string Name, string Identifier, List<Guid> LocationIds, Guid? ParentId = null);