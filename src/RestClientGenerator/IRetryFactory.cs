namespace RestClient;

public interface IRetryFactory
{
    IRetry CreateRetry(IHttpRequestContext httpRequestContext);
}
