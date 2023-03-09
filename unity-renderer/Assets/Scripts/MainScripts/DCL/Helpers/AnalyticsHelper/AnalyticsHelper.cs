using DCL;
using System.Collections.Generic;

public static class AnalyticsHelper
{
    public static void AddSceneNameAndBasePositionToDictionary(Dictionary<string, string> analyticDict)
    {
        IWorldState worldState = Environment.i.world.state;
        int sceneNumber = worldState.GetCurrentSceneNumber();
        string sceneHash = worldState.GetCurrentSceneHash();

        if (!worldState.TryGetScene(sceneNumber, out var scene))
            return;

        if (sceneNumber > 0 && !string.IsNullOrEmpty(sceneHash))
        {
            analyticDict.Add("base_parcel_position", scene.sceneData.basePosition.x + "," + scene.sceneData.basePosition.y );
            analyticDict.Add("scene", sceneHash);
        }
    }

    public static void SendExternalLinkAnalytic(string url, string nftToken)
    {
        Dictionary<string, string> eventToSend = new Dictionary<string, string>();
        eventToSend.Add("url", url);
        if (nftToken != null)
            eventToSend.Add("nft_token_id", nftToken);
        AddSceneNameAndBasePositionToDictionary(eventToSend);
        IAnalytics analytics = Environment.i.platform.serviceProviders.analytics;
        analytics.SendAnalytic("external_link_open", eventToSend);
    }

    public static void SendExternalLinkAnalytic(string url) { SendExternalLinkAnalytic(url, null); }
}
