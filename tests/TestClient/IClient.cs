namespace TestClient.Services;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestClient;

[GenerateContract]
[AddAuthorizationHeader()]
public interface IClient
{
    [Get("cluster/state/api/v1/workload/{id}")]
    [HttpResponseProcessor(typeof(ServiceResponseProcessor<WorkloadModel>))]
    [Retry(RetryLimit = 3, WaitTime = 250)]
    Task<IServiceResult<WorkloadModel>> GetWorkloadAsync(
        string id,
        CancellationToken cancellationToken = default);

    [OutputCode]
    [Post("cluster/state/api/v1/workload")]
    [Retry(RetryLimit = 3, WaitTime = 250, DoubleWaitTimeOnRetry = true, HttpStatusCodesToRetry = new[] { HttpStatusCode.ServiceUnavailable }, ExceptionTypesToRetry = new[] { typeof(Exception) })]
    [HttpResponseProcessor(typeof(ServiceResponseProcessor<string>))]
    Task<IServiceResult<string>> CreateWorkloadAsync(
        [SendAsContent()]
        CreateWorkloadModel model,
        CancellationToken cancellationToken = default);
}
