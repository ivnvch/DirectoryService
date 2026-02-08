namespace DirectoryService.Shared.Departments;

public record GetRootDepartmentDto
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }
    
    public required string Identifier { get; init; }
    
    public required string Path { get; init; }
    
    public Guid? ParentId { get; init; }
    
    public List<GetRootDepartmentDto> Children { get; init; } = [];
    
    public bool HasMoreChildren { get; init; }
}