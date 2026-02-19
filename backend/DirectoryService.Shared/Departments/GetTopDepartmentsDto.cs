namespace DirectoryService.Shared.Departments;

public record GetTopDepartmentDto(
   string Name,
   long CountPositions);
   
   public record GetTopDepartmentsDto(GetTopDepartmentDto[] topDepartments);