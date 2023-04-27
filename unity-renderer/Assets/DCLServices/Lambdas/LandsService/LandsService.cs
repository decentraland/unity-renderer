using Cysharp.Threading.Tasks;
using DCL;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.Lambdas.LandsService
{
    public class LandsService : ILandsService, ILambdaServiceConsumer<LandsResponse>
    {
        internal const string END_POINT = "users/";
        internal const int TIMEOUT = ILambdasService.DEFAULT_TIMEOUT;
        internal const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;

        private Service<ILambdasService> lambdasService;
        private readonly Dictionary<(string userId, int pageSize), LambdaResponsePagePointer<LandsResponse>> ownerLandsPagePointers = new ();

        public async UniTask<(IReadOnlyList<LandsResponse.LandEntry> lands, int totalAmount)> RequestOwnedLandsAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            // Start a transaction using Sentry SDK
            var transaction = SentrySdk.StartTransaction("RequestOwnedLandsAsync", "ILandsService");
            transaction.SetExtra("userId", userId);
            transaction.SetExtra("pageNumber", pageNumber.ToString());
            transaction.SetExtra("pageSize", pageSize.ToString());

            try
            {
                var createNewPointer = false;
                if (!ownerLandsPagePointers.TryGetValue((userId, pageSize), out var pagePointer))
                {
                    createNewPointer = true;
                }
                else if (cleanCachedPages)
                {
                    pagePointer.Dispose();
                    ownerLandsPagePointers.Remove((userId, pageSize));
                    createNewPointer = true;
                }

                if (createNewPointer)
                {
                    ownerLandsPagePointers[(userId, pageSize)] = pagePointer = new LambdaResponsePagePointer<LandsResponse>(
                        $"{END_POINT}{userId}/lands", pageSize, ct, this);
                }

                var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);

                if (!pageResponse.success)
                {
                    transaction.Status = SpanStatus.InternalError;
                    throw new Exception($"The request of the owned lands for '{userId}' failed!");
                }

                transaction.Status = SpanStatus.Ok;
                return (pageResponse.response.Elements, pageResponse.response.TotalAmount);
            }
            catch (Exception e)
            {
                // Set transaction status to the appropriate error
                transaction.Status = SpanStatus.InternalError;
                SentrySdk.CaptureException(e);
                throw;
            }
            finally
            {
                transaction.Finish();
            }
        }

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
