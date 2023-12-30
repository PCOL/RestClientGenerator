namespace TestClient;

/// <summary>
/// Defines the error model interface.
/// </summary>
public interface IErrorModel
{
    /// <summary>
    /// Gets the message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets a value indicating whether or not the request should be retried.
    /// </summary>
    bool? Retry { get; }
}