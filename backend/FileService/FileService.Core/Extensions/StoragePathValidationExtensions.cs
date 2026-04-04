using FluentValidation;
using Shared.Validation;

namespace FileService.Core.Extensions;

public static class StoragePathValidationExtensions
{
    public static IRuleBuilderOptionsConditions<T, string> ValidStoragePath<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.MustBeValueObject<T, string, (string BucketName, string ObjectKey)>(
            path => path.ParseStoragePath());
    }
}
