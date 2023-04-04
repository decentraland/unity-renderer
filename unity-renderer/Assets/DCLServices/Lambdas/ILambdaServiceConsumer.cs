using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCLServices.Lambdas
{
    public interface ILambdaServiceConsumer<TResponse> where TResponse : PaginatedResponse
    {
        UniTask<(TResponse response, bool success)> CreateRequest(string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken);
    }
}
