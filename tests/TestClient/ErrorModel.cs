namespace TestClient;

/// <summary>
/// Represents an error.
/// </summary>
public class ErrorModel
    : IErrorModel
{
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the operation should be retried.
    /// </summary>
    public bool? Retry { get; set; }
}