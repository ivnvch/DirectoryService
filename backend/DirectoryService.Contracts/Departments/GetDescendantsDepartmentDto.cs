namespace DirectoryService.Shared.Departments;

public record GetDescendantsDepartmentDto
{
    public required string Name { get; init; }
    
    public required string Identifier { get; init; }
    
    public required string Path { get; init; }
    
    public short Depth { get; init; }
    
    public Guid? ParentId { get; init; }
    
    public bool HasMoreChildren { get; init; }
}