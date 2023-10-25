namespace TestClient;

using System.Threading;
using System.Threading.Tasks;
using RestClient;

[GenerateContract]
public interface IClient
{
    [Get("cluster/api/v1/workload/{id}")]
    Task<string> GetWorkloadAsync(string id, CancellationToken cancellationToken = default);

    [Post("cluster/api/v1/workload")]
    Task<string> CreateWorkloadAsync(
        CreateWorkloadModel model,
        CancellationToken cancellationToken = default);
}
