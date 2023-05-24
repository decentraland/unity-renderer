using DCL.Controllers.LoadingScreenV2;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

public class HintSceneParser
{
    public static List<Hint> ParseJsonToHints(string json)
    {
        var hints = new List<Hint>();
        var sceneJson = JsonUtility.FromJson<LoadParcelScenesMessage.UnityParcelScene>(json);

        if (sceneJson == null || sceneJson.loadingScreenHints == null)
            return hints;

        foreach (var hint in sceneJson.loadingScreenHints)
        {
            hints.Add(new Hint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag));
        }
        return hints;
    }
}
