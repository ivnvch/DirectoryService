using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Shared.Constants;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Positions;

public sealed class Position
{
    private Position(){}
    private Position(Guid id, string name, string? description, IEnumerable<Guid> departments)
    {
        Id = id;
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        _departments = departments
            .Select(x => new DepartmentPosition(Guid.NewGuid(), Guid.NewGuid() ,x))
            .ToList();
    }
    
    private readonly List<DepartmentPosition> _departments = [];
    
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<DepartmentPosition> Departments => _departments;

    public static Result<Position, Error> Create(
        string name, 
        string? description, 
        IEnumerable<Guid> departments)
    {
        if (string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsInvalid("Name ");
        if (string.IsNullOrWhiteSpace(description) && description.Length > LengthConstant.Max1000Length)
            return GeneralErrors.ValueIsInvalid("Description");
        
        if(!departments.Any())
            return GeneralErrors.ValueIsInvalid("departments");
        

        return new Position(
            Guid.NewGuid(),
            name,
            description,
            departments);
    }
}