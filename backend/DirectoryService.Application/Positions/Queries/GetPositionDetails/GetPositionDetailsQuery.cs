using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Positions.Queries.GetPositionDetails;

public record GetPositionDetailsQuery(Guid Id) : IQuery;