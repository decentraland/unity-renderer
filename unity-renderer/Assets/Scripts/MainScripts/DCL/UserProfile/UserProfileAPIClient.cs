using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Threading;
using UnityEngine.Networking;

public interface IUserProfileAPIClient
{
    /// <summary>
    /// Fetch user profile json
    /// </summary>
    /// <param name="userId">eth address</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    UniTask<UserProfileModel> FetchUserProfile(string userId, CancellationToken ct);
    UniTask<string> FetchCatalystPublicKey(CancellationToken ct);
}

public class UserProfileAPIClient : IUserProfileAPIClient
{
    // https://peer.decentraland.zone/explorer/profiles/{id}
    // https://peer.decentraland.zone/about
    private const string URL_FETCH_USER_PROFILE = "https://peer.decentraland.zone/explorer/profiles/{user_id}"; // FD:: not the final URL
    private const string URL_FETCH_PUBLIC_KEY = "https://peer.decentraland.zone/about"; // FD:: not the final URL

    private Service<IWebRequestController> webRequestController;

    public async UniTask<UserProfileModel> FetchUserProfile(string userId, CancellationToken ct)
    {
        string url = URL_FETCH_USER_PROFILE.Replace("{user_id}", userId);
        var result = await webRequestController.Ref.GetAsync(url, cancellationToken: ct, isSigned: true);

        if (result.result != UnityWebRequest.Result.Success)
            throw new Exception($"Error fetching user profile:\n{result.error}");

        var response = Utils.SafeFromJson<UserProfileModel>(result.downloadHandler.text);

        if (response == null)
            throw new Exception($"Error parsing user profile:\n{result.downloadHandler.text}");

        return response;
    }

    public async UniTask<string> FetchCatalystPublicKey(CancellationToken ct)
    {
        var result = await webRequestController.Ref.GetAsync(URL_FETCH_PUBLIC_KEY, cancellationToken: ct, isSigned: true);

        if (result.result != UnityWebRequest.Result.Success)
            throw new Exception($"Error fetching catalyst public key:\n{result.error}");

        // FD:: is the public key part of the JSON response?
        var response = Utils.SafeFromJson<PublicKeyResponseModel>(result.downloadHandler.text);

        if (response == null || string.IsNullOrEmpty(response.PublicKey))
            throw new Exception($"Error parsing catalyst public key:\n{result.downloadHandler.text}");

        return response.PublicKey;
    }
}

public class PublicKeyResponseModel
{
    public string PublicKey;
}
