using DCL;
using System.Collections.Generic;

public static class AnalyticsHelper
{
    public static void AddSceneNameAndBasePositionToDictionary(Dictionary<string, string> analyticDict)
    {
        string sceneId = Environment.i.world.state.currentSceneId;
        if (!string.IsNullOrEmpty(sceneId))
        {
            analyticDict.Add("base_parcel_position", Environment.i.world.state.loadedScenes[sceneId].sceneData.basePosition.x + "," + Environment.i.world.state.loadedScenes[sceneId].sceneData.basePosition.y );
            analyticDict.Add("scene", sceneId);
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