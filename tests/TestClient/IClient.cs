namespace TestClient;

using System.Threading;
using System.Threading.Tasks;
using RestClientGenerator;

[GenerateContract]
public interface IClient
{
    [HttpGet("cluster/api/v1/workload/{id}")]
    Task<string> GetWorkloadAsync(string id, CancellationToken cancellationToken = default);
}
