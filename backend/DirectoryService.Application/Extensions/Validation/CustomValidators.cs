using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FluentValidation;

namespace DirectoryService.Application.Extensions.Validation;

public static class CustomValidators
{
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Error>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factoryMethod(value);

            if (result.IsSuccess)
                return;

            context.AddFailure(JsonSerializer.Serialize(result.Error));
        });
    }


    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
    {
        return rule.WithMessage(JsonSerializer.Serialize(error));
    }
}