using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers.LoadingScreenV2
{
    /// <summary>
    ///     The HintSceneParserUtil class is a utility class designed to handle the deserialization and conversion of JSON data into a list of Hint objects.
    ///     If the parsing process encounters null values or an absence of loading screen hints, it will return an empty list.
    /// </summary>
    public class HintSceneParserUtil
    {
        public static List<Hint> ParseJsonToHints(string json)
        {
            var hints = new List<Hint>();
            LoadParcelScenesMessage.UnityParcelScene sceneJson = JsonUtility.FromJson<LoadParcelScenesMessage.UnityParcelScene>(json);

            if (sceneJson == null || sceneJson.loadingScreenHints == null)
                return hints;

            foreach (Hint hint in sceneJson.loadingScreenHints) { hints.Add(new Hint(hint.TextureUrl, hint.Title, hint.Body, hint.SourceTag)); }

            return hints;
        }
    }
}
