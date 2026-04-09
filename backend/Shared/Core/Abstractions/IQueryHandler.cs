using CSharpFunctionalExtensions;
using Shared.CommonErrors;

namespace Shared.Abstractions;

public interface IQueryHandler<TResponse, in TQuery> where TQuery : IQuery
{
    Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IQueryHandler<TResponse> : IQuery
{
    Task<Result<TResponse, Errors>> Handle(CancellationToken cancellationToken);
}

public interface IListQueryHandler<TResponse, in TQuery> where TQuery : IQuery
{
    Task<Result<List<TResponse>, Errors>> HandleList(TQuery query, CancellationToken cancellationToken);
}