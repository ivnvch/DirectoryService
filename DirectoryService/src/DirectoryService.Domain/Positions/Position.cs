using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPostitions;
using DirectoryService.Shared.Constants;

namespace DirectoryService.Domain.Positions;

public class Position
{
    private Position(Guid id, string name, string? description, bool isActive, DateTime createdAt, DateTime? updatedAt, IEnumerable<Guid> positions)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _departmentPositions = positions
            .Select(x => new DepartmentPosition(Guid.NewGuid(), Guid.NewGuid() ,x))
            .ToList();
    }
    
    private readonly List<DepartmentPosition> _departmentPositions = [];
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

    public static Result<Position> Create(
        string name, 
        string? description, 
        bool isActive, 
        DateTime createdAt, 
        DateTime? updatedAt, 
        IEnumerable<Guid> positions)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Position>("Name cannot be empty");
        if (string.IsNullOrWhiteSpace(description) && description.Length > LengthConstant.Max1000Length)
            return Result.Failure<Position>("Description cannot be empty");
        
        if(!positions.Any())
            return Result.Failure<Position>("Positions cannot be empty");
        

        return new Position(
            Guid.NewGuid(),
            name,
            description,
            isActive,
            createdAt,
            updatedAt,
            positions);
    }
}