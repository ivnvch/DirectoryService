using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared.Errors;
using DirectoryService.Shared.Locations.ResponseModels;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsDto, GetLocationsQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetLocationsQuery> _validator;

    public GetLocationsHandler(IReadDbContext readDbContext, IValidator<GetLocationsQuery> validator)
    {
        _readDbContext = readDbContext;
        _validator = validator;
    }

    public async Task<Result<GetLocationsDto, Errors>> Handle(GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
        
        var queryResult = _readDbContext.LocationsRead;

        if (!string.IsNullOrWhiteSpace(query.Search))
            queryResult = queryResult.Where(x =>
                EF.Functions.Like(x.Name.Value.ToLower(), $"%{query.Search.ToLower()}%"));
        
        if (query.IsActive.HasValue)
            queryResult = queryResult.Where(x => x.IsActive == query.IsActive.Value);

        if (query.DepartmentIds is { Length: > 0 })
            queryResult = queryResult
                .Where(x => x.Departments.Any(d => query.DepartmentIds.Contains(d.DepartmentId)));

        Expression<Func<Location, object>> keySelector = query.SortBy?.ToLower() switch
        {
            "name" => x => x.Name,
            "date" => x => x.CreatedAt,
            _ => x => x.CreatedAt
        };
        
        queryResult = query.SortDirection == "asc"
            ? queryResult.OrderBy(keySelector)
            : queryResult.OrderByDescending(keySelector);
            
        queryResult = queryResult.OrderBy(keySelector);
        
        var totalCount = await queryResult.LongCountAsync(cancellationToken);
        
        queryResult = queryResult
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize);

         var result = await queryResult
                .Select(x => new GetLocationDto
                {
                    Name = x.Name.Value,
                    Address =
                        $"{x.Address.Country}, {x.Address.City}, {x.Address.Street}, {x.Address.House}, {x.Address.Apartment}",
                    CreatedAt = x.CreatedAt,
                    Timezone = x.Timezone.Value,
                    IsActive = x.IsActive
                }).ToListAsync(cancellationToken);


         return new GetLocationsDto(result, totalCount);
    }
}