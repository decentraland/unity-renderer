using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;

namespace DCLServices.Lambdas.LandsService
{
    public class LandsService : ILandsService, ILambdaServiceConsumer<LandsResponse>
    {
        internal const string END_POINT = "nfts/lands/";
        internal const int TIMEOUT = ILambdasService.DEFAULT_TIMEOUT;
        internal const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;

        private Service<ILambdasService> lambdasService;

        public LambdaResponsePagePointer<LandsResponse> GetPaginationPointer(string address, int pageSize, CancellationToken ct) =>
            new (END_POINT + address, pageSize, ct, this);

        UniTask<(LandsResponse response, bool success)> ILambdaServiceConsumer<LandsResponse>.CreateRequest(string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            lambdasService.Ref.Get<LandsResponse>(
                END_POINT,
                endPoint,
                TIMEOUT,
                ATTEMPTS_NUMBER,
                cancellationToken,
                LambdaPaginatedResponseHelper.GetPageSizeParam(pageSize), LambdaPaginatedResponseHelper.GetPageNumParam(pageNumber));

        public void Dispose() { }

        public void Initialize() { }
    }
}
