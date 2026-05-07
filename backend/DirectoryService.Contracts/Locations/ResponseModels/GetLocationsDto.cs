namespace DirectoryService.Shared.Locations.ResponseModels;

public record GetLocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Address { get; init; }
    public string Timezone {get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
