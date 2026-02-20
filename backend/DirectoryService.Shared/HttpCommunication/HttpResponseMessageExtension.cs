using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Shared.HttpCommunication;

public static class HttpResponseMessageExtension
{
    public static async Task<Result<TResponse, Errors.Errors>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    where TResponse : class
    {
        try
        {
            Envelope<TResponse>? envelopeResponse =  await response.Content
                .ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return envelopeResponse.ErrorsList ?? GeneralErrors.Failure("Error while processing response");
            }

            if (envelopeResponse is null)
            {
                return GeneralErrors.Failure("Error while processing response").ToErrors();
            }

            if (envelopeResponse.IsError)
            {
                return envelopeResponse.ErrorsList;
            }

            if (envelopeResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while processing response").ToErrors();
            }

           return envelopeResponse.Result;
        }
        catch
        {
           return GeneralErrors.Failure("Error while processing response").ToErrors();
        }
    }
    
    public static async Task<UnitResult<Errors.Errors>> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Envelope? envelopeResponse =  await response.Content
                .ReadFromJsonAsync<Envelope>(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return envelopeResponse?.ErrorList ?? GeneralErrors.Failure("Error while processing response");
            }

            if (envelopeResponse is null)
            {
                return GeneralErrors.Failure("Error while processing response").ToErrors();
            }

            if (envelopeResponse.ErrorList is not null)
            {
                return envelopeResponse.ErrorList;
            }

            if (envelopeResponse.Result is null)
            {
                return GeneralErrors.Failure("Error while processing response").ToErrors();
            }

            return UnitResult.Success<Errors.Errors>();
        }
        catch
        {
            return GeneralErrors.Failure("Error while processing response").ToErrors();
        }
    }
}