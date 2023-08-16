using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.Lambdas
{
    public interface ILambdaServiceConsumer<TResponse> where TResponse : PaginatedResponse
    {
        UniTask<(TResponse response, bool success)> CreateRequest(string endPoint, int pageSize, int pageNumber, Dictionary<string, string> additionalData = null, CancellationToken cancellationToken = default);
    }
}
