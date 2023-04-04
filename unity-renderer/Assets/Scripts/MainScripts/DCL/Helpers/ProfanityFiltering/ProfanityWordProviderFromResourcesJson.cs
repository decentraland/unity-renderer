using System.Collections.Generic;
using UnityEngine;

namespace DCL.ProfanityFiltering
{
    public class ProfanityWordProviderFromResourcesJson : IProfanityWordProvider
    {
        private readonly string jsonFilePath;

        public ProfanityWordProviderFromResourcesJson(string jsonFilePath)
        {
            this.jsonFilePath = jsonFilePath;
        }

        public IEnumerable<string> GetExplicitWords()
        {
            var asset = Resources.Load<TextAsset>(jsonFilePath);
            var json = JsonUtility.FromJson<ProfanityWordsJsonStructure>(asset.text);
            return json.explicitWords;
        }

        public IEnumerable<string> GetNonExplicitWords()
        {
            var asset = Resources.Load<TextAsset>(jsonFilePath);
            var json = JsonUtility.FromJson<ProfanityWordsJsonStructure>(asset.text);
            return json.nonExplicitWords;
        }

        private struct ProfanityWordsJsonStructure
        {
            public string[] explicitWords;
            public string[] nonExplicitWords;
        }
    }
}
