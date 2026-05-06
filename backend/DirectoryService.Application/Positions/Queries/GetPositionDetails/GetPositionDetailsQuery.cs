using Shared.Abstractions;

namespace DirectoryService.Application.Positions.Queries.GetPositionDetails;

public record GetPositionDetailsQuery(Guid Id) : IQuery;