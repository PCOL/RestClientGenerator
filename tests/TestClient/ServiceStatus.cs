namespace TestClient;

/// <summary>
/// Service status.
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// The operation was successful, but the request is still being processed by the server.
    /// </summary>
    Accepted,

    /// <summary>
    /// The operation was successful, but no data was returned.
    /// </summary>
    SuccessNoData,

    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success,

    /// <summary>
    /// The operation was successfully created.
    /// </summary>
    SuccessCreated,

    /// <summary>
    /// The request was bad.
    /// </summary>
    BadRequest,

    /// <summary>
    /// The request was unauthorized.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// The request was forbidden.
    /// </summary>
    Forbidden,

    /// <summary>
    /// The resource was not found.
    /// </summary>
    ResourceNotFound,

    /// <summary>
    /// The resource already exists.
    /// </summary>
    ResourceExists,

    /// <summary>
    /// A downstream service was not found.
    /// </summary>
    ServiceNotFound,

    /// <summary>
    /// The server is too busy.
    /// </summary>
    TooManyRequests,

    /// <summary>
    /// The version already exists.
    /// </summary>
    PreconditionFailed,

    /// <summary>
    /// The service is unavailable.
    /// </summary>
    ServiceUnavailable,

    /// <summary>
    /// An error occurred.
    /// </summary>
    Error,

    /// <summary>
    /// The operation was cancelled.
    /// </summary>
    Cancelled
}
