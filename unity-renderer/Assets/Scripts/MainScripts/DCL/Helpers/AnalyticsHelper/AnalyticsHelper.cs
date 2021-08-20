using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using UnityEngine;

public static class AnalyticsHelper
{
    private static IParcelScene GetSceneWherePlayerIsStanding()
    {
        foreach (IParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
        {
            if (WorldStateUtils.IsCharacterInsideScene(scene))
                return scene;
        }
        return null;
    }

    public static void AddSceneNameAndBasePositionToDictionary(Dictionary<string, string> analyticDict)
    {
        IParcelScene scene = GetSceneWherePlayerIsStanding();
        analyticDict.Add("parcel", scene.sceneData.basePosition.ToString());
        analyticDict.Add("scene", scene.GetName());
    }

    public static void SendExternalLinkAnalytic(string url, string nftToken)
    {
        Dictionary<string, string> eventToSend = new Dictionary<string, string>();
        eventToSend.Add("url", url);
        if (nftToken != null)
            eventToSend.Add("nft_token_id", nftToken);
        AddSceneNameAndBasePositionToDictionary(eventToSend);
        Analytics.i.SendAnalytic("external_link_open", eventToSend);
    }

    public static void SendExternalLinkAnalytic(string url) { SendExternalLinkAnalytic(url, null); }
}