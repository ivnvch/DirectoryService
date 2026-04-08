using Microsoft.AspNetCore.Routing;

namespace Shared.EndpointResults;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder builder);
}