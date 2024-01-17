namespace TestClient.Services;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestClient;

[GenerateContract]
public interface IClient
{
    [Get("cluster/state/api/v1/workload/{id}")]
    [HttpResponseProcessor(typeof(ServiceResponseProcessor<WorkloadModel>))]
    [Retry(RetryLimit = 3, WaitTime = 250)]
    Task<IServiceResult<WorkloadModel>> GetWorkloadAsync(
        [SendAsHeader("Authorization", Format = "bearer {0}")]
        string token,
        string id,
        CancellationToken cancellationToken = default);

    [Post("cluster/state/api/v1/workload")]
    [AddAuthorizationHeader(typeof(AuthorizationFactory))]
    [Retry(RetryLimit = 3, WaitTime = 250, DoubleWaitTimeOnRetry = true, HttpStatusCodesToRetry = new[] { HttpStatusCode.ServiceUnavailable }, ExceptionTypesToRetry = new[] { typeof(Exception) })]
    [HttpResponseProcessor(typeof(ServiceResponseProcessor<string>))]
    Task<IServiceResult<string>> CreateWorkloadAsync(
        [SendAsContent()]
        CreateWorkloadModel model,
        CancellationToken cancellationToken = default);
}
