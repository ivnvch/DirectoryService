using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.Positions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Positions.Queries.GetPositions;

public class GetPositionsHandler : IQueryHandler<PaginationResponse<GetPositionDto>, GetPositionQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetPositionQuery> _validator;
    

    public GetPositionsHandler(IValidator<GetPositionQuery> validator, IReadDbContext readDbContext)
    {
        _validator = validator;
        _readDbContext = readDbContext;
    }

    public async Task<Result<PaginationResponse<GetPositionDto>, Errors>> Handle(GetPositionQuery query, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();

        var queryResult = _readDbContext.PositionsRead;
        
        if (!string.IsNullOrWhiteSpace(query.Search))
            queryResult = queryResult.Where(p => 
                EF.Functions.Like(p.Name.ToLower(), $"%{query.Search.ToLower()}%"));
        
        if (query.IsActive.HasValue)
            queryResult = queryResult.Where(p => p.IsActive == query.IsActive.Value);
        
        if (query.DepartmentIds is {Length: > 0})
            queryResult = queryResult.Where(p => p.Departments.Any(d => query.DepartmentIds.Contains(d.DepartmentId)));

        Expression<Func<Position, object>> keySelector = query.SortBy?.ToLower() switch
        {
            "name" => x => x.Name,
            "date" => x => x.CreatedAt,
            _ => x => x.CreatedAt
        };
        
        queryResult = query.SortDirection == "asc"
            ? queryResult.OrderBy(keySelector)
            : queryResult.OrderByDescending(keySelector);
        
        var totalCount = await queryResult.CountAsync(cancellationToken);

        queryResult = queryResult
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);
        
        var result = await queryResult
            .Select(x => new GetPositionDto(
                x.Id,
                x.Name,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt!.Value)).ToListAsync(cancellationToken);
        
        int totalPages = (totalCount + query.PageSize - 1) / query.PageSize;

        return new PaginationResponse<GetPositionDto>(
            result,
            totalCount,
            query.Page,
            query.PageSize,
            totalPages);
    }
}