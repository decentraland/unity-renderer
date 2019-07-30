using DCL;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarRandomizer
{
    public const string AVATAR_ENDPOINT_URL = "https://avatar-assets.now.sh/";
    public const string CONTENT_ENDPOINT_URL = "https://s3.amazonaws.com/content-service.decentraland.zone/";

    [System.Serializable]
    public class WearableContent
    {
        public string file;
        public string name;
    }

    [System.Serializable]
    public class i18n
    {
        public string en;
        public string es;
    }

    [System.Serializable]
    public class WearableModelPair
    {
        public string type;
        public string model;
    }

    [System.Serializable]
    public class WearableAsset
    {
        public string thumbnail;
        public WearableContent[] contents;
        public string path;
        public string id;
        public string name;
        public string[] tags;
        public string category;
        public i18n i18n;
        public WearableModelPair[] main;
    }

    [System.Serializable]
    public class AvatarAssetsResponse
    {
        public bool ok;
        public WearableAsset[] data;
    }

    AvatarAssetsResponse model;

    class WearableSet
    {
        public AvatarShape.Model.Wearable bodyShape = new AvatarShape.Model.Wearable();
        public Dictionary<string, List<AvatarShape.Model.Wearable>> categoryToWearableList = new Dictionary<string, List<AvatarShape.Model.Wearable>>();
        public List<AvatarShape.Model.Face> eyesList = new List<AvatarShape.Model.Face>();
        public List<AvatarShape.Model.Face> mouthList = new List<AvatarShape.Model.Face>();
        public List<AvatarShape.Model.Face> eyebrowsList = new List<AvatarShape.Model.Face>();
    }

    WearableSet maleSet = new WearableSet();
    WearableSet femaleSet = new WearableSet();

    string[] faceCategories = new string[] { AvatarShape.CATEGORY_EYES, AvatarShape.CATEGORY_EYEBROWS, AvatarShape.CATEGORY_MOUTH };

    Color[] hairColorPresets = new Color[5] { new Color(0.1f, 0.1f, 0.1f, 1), new Color(0.36f, 0.19f, 0.06f, 1), new Color(0.54f, 0.12f, 0.08f, 1), new Color(1, 0.74f, 0.15f, 1), new Color(0.92f, 0.90f, 0.88f, 1) };
    Color[] skinColorPresets = new Color[5] { new Color(1f, 0.86f, 0.73f, 1), new Color(0.86f, 0.69f, 0.56f, 1), new Color(0.80f, 0.60f, 0.46f, 1), new Color(0.49f, 0.36f, 0.27f, 1), new Color(0.32f, 0.17f, 0.10f, 1) };

    public AvatarRandomizer()
    {
    }

    public AvatarShape.Model GetRandomModel(int seed)
    {
        Random.InitState(seed);
        AvatarShape.Model result = new AvatarShape.Model();

        result.baseUrl = CONTENT_ENDPOINT_URL;
        result.useDummyModel = false;

        WearableSet set = null;

        if (Random.Range(0, 100) < 50)
        {
            set = maleSet;
        }
        else
        {
            set = femaleSet;
        }

        result.bodyShape = set.bodyShape;
        var wearables = new List<AvatarShape.Model.Wearable>();

        result.hair = new AvatarShape.Model.Hair() { color = hairColorPresets[Random.Range(0, hairColorPresets.Length)] };
        result.skin = new AvatarShape.Model.Skin() { color = skinColorPresets[Random.Range(0, skinColorPresets.Length)] };
        result.eyes = new AvatarShape.Model.Eyes() { color = Random.ColorHSV(0, 1, 0.5f, 1, 0.1f, 0.5f) };
        result.eyebrows = new AvatarShape.Model.Face() { };
        result.mouth = new AvatarShape.Model.Face() { };

        foreach (var kvPair in set.categoryToWearableList)
        {
            if (faceCategories.Contains(kvPair.Key))
                continue;

            var wearablesList = kvPair.Value;
            int randomWearableIndex = Random.Range(0, wearablesList.Count - 1);

            wearables.Add(wearablesList[randomWearableIndex]);
        }

        {
            int randomIndex = Random.Range(0, set.eyesList.Count - 1);
            result.eyes.texture = set.eyesList[randomIndex].texture;
            result.eyes.mask = set.eyesList[randomIndex].mask;
        }

        {
            int randomIndex = Random.Range(0, set.mouthList.Count - 1);
            result.mouth.texture = set.mouthList[randomIndex].texture;
            result.mouth.mask = set.mouthList[randomIndex].mask;
        }

        {
            int randomIndex = Random.Range(0, set.eyebrowsList.Count - 1);
            result.eyebrows.texture = set.eyebrowsList[randomIndex].texture;
            result.eyebrows.mask = set.eyebrowsList[randomIndex].mask;
        }

        result.wearables = wearables.ToArray();

        return result;
    }

    public IEnumerator FetchAllAvatarAssets()
    {
        if (model != null)
            yield break;

        UnityWebRequest req = UnityWebRequest.Get(AVATAR_ENDPOINT_URL);
        yield return req.SendWebRequest();

        model = JsonUtility.FromJson<AvatarAssetsResponse>(req.downloadHandler.text);

        foreach (WearableAsset a in model.data)
        {
            WearableSet targetSet = null;
            var newWearable = new AvatarShape.Model.Wearable();
            var mappingList = new List<ContentProvider.MappingPair>();

            foreach (WearableContent content in a.contents)
            {
                mappingList.Add(new ContentProvider.MappingPair() { file = content.name, hash = content.file });
            }

            newWearable.contents = mappingList.ToArray();
            newWearable.category = a.category;

            foreach (WearableModelPair pair in a.main)
            {
                newWearable.contentName = pair.model;

                if (pair.type.ToLower().Contains("female"))
                {
                    targetSet = femaleSet;
                }
                else
                {
                    targetSet = maleSet;
                }
            }

            if (a.category == "body_shape")
            {
                if (a.name == "female_body")
                {
                    femaleSet.bodyShape = newWearable;
                }
                else
                {
                    maleSet.bodyShape = newWearable;
                }

                continue;
            }

            if (faceCategories.Contains(a.category))
            {
                var face = new AvatarShape.Model.Face();
                face.texture = a.contents.FirstOrDefault((x) => x.name.ToLower().Contains(a.category.ToLower()))?.file;
                face.mask = a.contents.FirstOrDefault((x) => x.name.ToLower().Contains("mask"))?.file;

                switch (a.category)
                {
                    case AvatarShape.CATEGORY_EYES:
                        targetSet.eyesList.Add(face);
                        break;
                    case AvatarShape.CATEGORY_EYEBROWS:
                        targetSet.eyebrowsList.Add(face);
                        break;
                    case AvatarShape.CATEGORY_MOUTH:
                        targetSet.mouthList.Add(face);
                        break;
                }
            }

            if (!targetSet.categoryToWearableList.ContainsKey(newWearable.category))
            {
                targetSet.categoryToWearableList.Add(newWearable.category, new List<AvatarShape.Model.Wearable>());
            }

            targetSet.categoryToWearableList[newWearable.category].Add(newWearable);
        }
    }
}
