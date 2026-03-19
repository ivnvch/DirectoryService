using CSharpFunctionalExtensions;

namespace Shared.Abstractions;

public interface IQueryHandler<TResponse, in TQuery> where TQuery : IQuery
{
    Task<Result<TResponse, Shared.Errors.Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandler<TResponse> : IQuery
{
    Task<Result<TResponse, Shared.Errors.Errors>> Handle(CancellationToken cancellationToken);
}

public interface IListQueryHandler<TResponse, in TQuery> where TQuery : IQuery
{
    Task<Result<List<TResponse>, Shared.Errors.Errors>> HandleList(TQuery query, CancellationToken cancellationToken);
}