using MainScripts.DCL.ServiceProviders.OpenSea.RequestScheduler;
using System;

namespace MainScripts.DCL.ServiceProviders.OpenSea.RequestHandlers
{
    internal interface IRequestHandler
    {
        SchedulableRequestHandler schedulableRequestHandler { get; }
        string GetUrl();
        void SetApiResponse(string responseJson, Action onSuccess, Action<string> onError);
        void SetApiResponseError(string error);
        bool CanRetry();
        void Retry();
    }
}
