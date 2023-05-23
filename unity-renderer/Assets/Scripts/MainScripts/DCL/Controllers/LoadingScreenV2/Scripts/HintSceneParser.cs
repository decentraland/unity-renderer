using DCL.Controllers.LoadingScreenV2;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

public class HintSceneParser
{
    public static List<IHint> ParseJsonToHints(string json)
    {
        var hints = new List<IHint>();
        var sceneJson = JsonUtility.FromJson<LoadParcelScenesMessage.UnityParcelScene>(json);

        if (sceneJson == null || sceneJson.loadingScreenHints == null)
            return hints;

        foreach (var hint in sceneJson.loadingScreenHints)
        {
            hints.Add(new BaseHint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag));
        }
        return hints;
    }
}
