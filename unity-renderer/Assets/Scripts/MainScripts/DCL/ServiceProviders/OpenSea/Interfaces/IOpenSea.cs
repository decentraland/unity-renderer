using System;
using System.Collections;

namespace MainScripts.DCL.ServiceProviders.OpenSea.Interfaces
{
    public interface IOpenSea : IDisposable
    {
        IEnumerator FetchNFTInfo(string assetContractAddress, string tokenId, Action<NFTInfo> onSuccess, Action<string> onError);

        IEnumerator FetchNFTsFromOwner(string assetContractAddress, Action<OwnNFTInfo> onSuccess, Action<string> onError);
    }
}
