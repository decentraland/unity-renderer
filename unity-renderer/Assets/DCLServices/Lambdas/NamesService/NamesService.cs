using Cysharp.Threading.Tasks;
using DCL;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.Lambdas.NamesService
{
    public class NamesService : INamesService, ILambdaServiceConsumer<NamesResponse>
    {
        internal const string END_POINT = "users/";
        internal const int TIMEOUT = ILambdasService.DEFAULT_TIMEOUT;
        internal const int ATTEMPTS_NUMBER = ILambdasService.DEFAULT_ATTEMPTS_NUMBER;

        private Service<ILambdasService> lambdasService;
        private readonly Dictionary<(string userId, int pageSize), LambdaResponsePagePointer<NamesResponse>> ownerNamesPagePointers = new ();

        public async UniTask<(IReadOnlyList<NamesResponse.NameEntry> names, int totalAmount)> RequestOwnedNamesAsync(string userId, int pageNumber, int pageSize, bool cleanCachedPages, CancellationToken ct)
        {
            var transaction = SentrySdk.StartTransaction("RequestOwnedNames", "INamesService");
            // Set the 'userId' tag in the transaction context + other tags
            transaction.SetExtra("userId", userId);
            transaction.SetExtra("pageNumber", pageNumber.ToString());
            transaction.SetExtra("pageSize", pageSize.ToString());

            try
            {
                var createNewPointer = false;
                if (!ownerNamesPagePointers.TryGetValue((userId, pageSize), out var pagePointer))
                {
                    createNewPointer = true;
                }
                else if (cleanCachedPages)
                {
                    pagePointer.Dispose();
                    ownerNamesPagePointers.Remove((userId, pageSize));
                    createNewPointer = true;
                }

                if (createNewPointer)
                {
                    ownerNamesPagePointers[(userId, pageSize)] = pagePointer = new LambdaResponsePagePointer<NamesResponse>(
                        $"{END_POINT}{userId}/names", pageSize, ct, this);
                }

                var pageResponse = await pagePointer.GetPageAsync(pageNumber, ct);

                if (!pageResponse.success)
                {
                    transaction.Status = SpanStatus.InternalError;
                    throw new Exception($"The request of the owned names for '{userId}' failed!");
                }

                transaction.Status = SpanStatus.Ok;
                return (pageResponse.response.Elements, pageResponse.response.TotalAmount);
            }
            catch (Exception ex)
            {
                // Set transaction status to the appropriate error
                transaction.Status = SpanStatus.InternalError;
                SentrySdk.CaptureException(ex);
                throw;
            }
            finally
            {
                transaction.Finish();
            }
        }

        UniTask<(NamesResponse response, bool success)> ILambdaServiceConsumer<NamesResponse>.CreateRequest(string endPoint, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
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
