namespace TestClient;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestClient;

[GenerateContract]
public interface IClient
{
    [Get("cluster/state/api/v1/workload/{id}")]
    Task<string> GetWorkloadAsync(string id, CancellationToken cancellationToken = default);

    [Post("cluster/state/api/v1/workload")]
    [Retry(RetryCount = 3, WaitTime = 250, DoubleWaitTimeOnRetry = true, HttpStatusCodesToRetry = new[] { HttpStatusCode.ServiceUnavailable })]
    [HttpResponseProcessor(typeof(ServiceResponseProcessor<string>))]
    Task<IServiceResult<string>> CreateWorkloadAsync(
        [SendAsContent()]
        CreateWorkloadModel model,
        CancellationToken cancellationToken = default);
}
