namespace TestClient;

/// <summary>
/// Implemenetation of the <see cref="IServiceResult"/> interface.
/// </summary>
public class ServiceResult
    : IServiceResult
{
    /// <inheritdoc />
    public ServiceStatus Status { get; set; }

    /// <inheritdoc />
    public bool IsError { get; set; }

    /// <inheritdoc />
    public IErrorModel Error { get; set; }
}