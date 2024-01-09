namespace RestClientGeneratorUnitTests;

using System.Net;

/// <summary>
/// Service extensions methods.
/// </summary>
public static class RestResultExtensionMethods
{
    /// <summary>
    /// Converts a <see cref="HttpStatusCode"/> into a <see cref="Status"/>.
    /// </summary>
    /// <param name="statusCode">A status code.</param>
    /// <returns>A service status.</returns>
    public static RestStatus ToStatus(this HttpStatusCode statusCode)
    {
        switch (statusCode)
        {
            case HttpStatusCode.OK:
                return RestStatus.Success;

            case HttpStatusCode.Created:
                return RestStatus.SuccessCreated;

            case HttpStatusCode.NoContent:
                return RestStatus.SuccessNoData;

            case HttpStatusCode.Accepted:
                return RestStatus.Accepted;

            case HttpStatusCode.Forbidden:
                return RestStatus.Forbidden;

            case HttpStatusCode.NotFound:
                return RestStatus.ResourceNotFound;

            case HttpStatusCode.Conflict:
                return RestStatus.ResourceExists;

            case HttpStatusCode.BadGateway:
                return RestStatus.ServiceNotFound;

            case HttpStatusCode.PreconditionFailed:
                return RestStatus.PreconditionFailed;

            case HttpStatusCode.BadRequest:
                return RestStatus.BadRequest;

            case HttpStatusCode.TooManyRequests:
                return RestStatus.TooManyRequests;

            default:
                return RestStatus.Error;
        }
    }
}