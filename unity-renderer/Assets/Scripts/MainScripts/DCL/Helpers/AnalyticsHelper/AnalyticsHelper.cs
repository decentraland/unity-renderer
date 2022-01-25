using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using UnityEngine;

public static class AnalyticsHelper
{
    public static void AddSceneNameAndBasePositionToDictionary(Dictionary<string, string> analyticDict)
    {
        string sceneId = Environment.i.world.state.currentSceneId;
        if (!string.IsNullOrEmpty(sceneId))
        {
            analyticDict.Add("parcel", Environment.i.world.state.loadedScenes[sceneId].sceneData.basePosition.x + "," + Environment.i.world.state.loadedScenes[sceneId].sceneData.basePosition.y );
            analyticDict.Add("scene", sceneId);
        }
    }

    public static void SendVoiceChatStartedAnalytic()
    {
        Dictionary<string, string> eventToSend = new Dictionary<string, string>();
        AddSceneNameAndBasePositionToDictionary(eventToSend);
        IAnalytics analytics = Environment.i.platform.serviceProviders.analytics;
        analytics.SendAnalytic("voice_chat_start_recording", eventToSend);
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