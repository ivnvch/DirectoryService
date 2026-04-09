using Amazon.S3;
using FileService.Domain;
using Shared.CommonErrors;

namespace FileService.Infrastructure.S3;

public static class S3ErrorMapper
{
    public static Error ToError(Exception ex) => ex switch
    {
        AmazonS3Exception { ErrorCode: "NoSuchBucket" }
            => FileErrors.BucketNotFound(),

        AmazonS3Exception { ErrorCode: "NoSuchKey" } => FileErrors.ObjectNotFound(),

        AmazonS3Exception { ErrorCode: "AccessDenied" } => GeneralErrors.Forbidden(),

        AmazonS3Exception { ErrorCode: "InvalidObjectState" }
            => Error.Conflict("s3.invalid.object.state", "Object state is invalid for this operation"),

        _ => FileErrors.InternalServerError()
    };
}