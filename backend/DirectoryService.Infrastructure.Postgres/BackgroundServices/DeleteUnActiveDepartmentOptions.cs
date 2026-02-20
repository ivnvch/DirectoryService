namespace DirectoryService.Infrastructure.BackgroundServices;

public record DeleteUnActiveDepartmentOptions
{
    public TimeSpan RunAtUtc { get; init; } = TimeSpan.FromHours(2);
    public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(30);
}