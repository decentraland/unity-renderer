using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;

namespace DCLServices.Lambdas.NamesService
{
    public class NamesService : INamesService, ILambdaServiceConsumer<NamesResponse>
    {
        internal const string END_POINT = "nfts/names/";
        internal const int TIMEOUT = ILambdasService.DEFAULT_TIMEOUT;
        internal const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;

        private Service<ILambdasService> lambdasService;

        public LambdaResponsePagePointer<NamesResponse> GetPaginationPointer(string address, int pageSize, CancellationToken ct) =>
            new (END_POINT + address, pageSize, ct, this);

        UniTask<(NamesResponse response, bool success)> ILambdaServiceConsumer<NamesResponse>.CreateRequest
            (string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Ref.Get<NamesResponse>(
                END_POINT,
                endPoint,
                TIMEOUT,
                ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize), LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber));

        public void Initialize() { }

        public void Dispose() { }
    }
}
