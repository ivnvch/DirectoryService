namespace DirectoryService.Shared.Departments;

public record GetShortDepartmentDto(
    string Name, 
    string Identifier, 
    string Path);