namespace TestClient;

using System.Net;

/// <summary>
/// Service extensions methods.
/// </summary>
public static class ServiceResultExtensionMethods
{
    /// <summary>
    /// Converts a <see cref="HttpStatusCode"/> into a <see cref="ServiceStatus"/>.
    /// </summary>
    /// <param name="statusCode">A status code.</param>
    /// <returns>A service status.</returns>
    public static ServiceStatus ToStatus(this HttpStatusCode statusCode)
    {
        switch (statusCode)
        {
            case HttpStatusCode.OK:
                return ServiceStatus.Success;

            case HttpStatusCode.Created:
                return ServiceStatus.SuccessCreated;

            case HttpStatusCode.NoContent:
                return ServiceStatus.SuccessNoData;

            case HttpStatusCode.Accepted:
                return ServiceStatus.Accepted;

            case HttpStatusCode.Forbidden:
                return ServiceStatus.Forbidden;

            case HttpStatusCode.NotFound:
                return ServiceStatus.ResourceNotFound;

            case HttpStatusCode.Conflict:
                return ServiceStatus.ResourceExists;

            case HttpStatusCode.BadGateway:
                return ServiceStatus.ServiceNotFound;

            case HttpStatusCode.PreconditionFailed:
                return ServiceStatus.PreconditionFailed;

            case HttpStatusCode.BadRequest:
                return ServiceStatus.BadRequest;

            case HttpStatusCode.TooManyRequests:
                return ServiceStatus.TooManyRequests;

            default:
                return ServiceStatus.Error;
        }
    }
}