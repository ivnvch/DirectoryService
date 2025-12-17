using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Shared.Constants;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Positions;

public sealed class Position
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

    public static Result<Position, Error> Create(
        string name, 
        string? description, 
        bool isActive, 
        DateTime createdAt, 
        DateTime? updatedAt, 
        IEnumerable<Guid> positions)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsInvalid("Name ");
        if (string.IsNullOrWhiteSpace(description) && description.Length > LengthConstant.Max1000Length)
            return GeneralErrors.ValueIsInvalid("Description");
        
        if(!positions.Any())
            return GeneralErrors.ValueIsInvalid("Positions");
        

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