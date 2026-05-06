using DirectoryService.Shared.Departments;

namespace DirectoryService.Shared.Positions;

public record GetPositionDetailsDto(
    GetPositionDto? Position,
    IReadOnlyList<GetShortDepartmentDto> Departments);