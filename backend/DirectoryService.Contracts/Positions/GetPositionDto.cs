namespace DirectoryService.Shared.Positions;

public record GetPositionDto(
        Guid Id,
        string Name,
        string? Description,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);