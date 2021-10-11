using System;

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
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