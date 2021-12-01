using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AvatarSystem;
using UnityEngine;
using Avatar = AvatarSystem.Avatar;

//TODO REMOVE THIS TESTING CLASS, IF YOU SEE THIS IN A REVIEW, HIT ME
public class TestAvatarSystem : MonoBehaviour
{

    public TextAsset testAvatar;

    [ContextMenu("Load")]
    public void Load()
    {
        StopAllCoroutines();
        StartCoroutine(MyLoad());
    }

    public IEnumerator MyLoad()
    {
        var avatarModel = JsonUtility.FromJson<AvatarModel>(testAvatar.text);

        Dictionary<string, WearableItem> wearables = new Dictionary<string, WearableItem>();
        WearableItem bodyshape = null;
        WearableItem eyes = null;
        WearableItem eyebrows = null;
        WearableItem mouth = null;
        yield return CatalogController.RequestWearable(avatarModel.bodyShape).Then(x => bodyshape = x);
        foreach (string wearable in avatarModel.wearables)
        {
            var promise = CatalogController.RequestWearable(wearable)
                                           .Then(x =>
                                           {
                                               switch (x.data.category)
                                               {
                                                   case WearableLiterals.Categories.EYES:
                                                       eyes = x;
                                                       return;
                                                   case WearableLiterals.Categories.EYEBROWS:
                                                       eyebrows = x;
                                                       return;
                                                   case WearableLiterals.Categories.MOUTH:
                                                       mouth = x;
                                                       return;
                                                   default:
                                                       wearables.Add(x.data.category, x);
                                                       break;
                                               }
                                           });
            promise.Catch(Debug.LogError);
            yield return promise;
        }

        Loader loader = new Loader(new WearableLoaderFactory(), gameObject);
        Avatar avatar = new Avatar(loader);
        yield return loader.Load(bodyshape, eyes, eyebrows, mouth, wearables.Values.ToList(), new AvatarSettings
        {
            bodyshapeId = avatarModel.bodyShape,
            eyesColor = avatarModel.eyeColor,
            hairColor = avatarModel.hairColor,
            skinColor = avatarModel.skinColor,
            headVisible = true,
            upperbodyVisible = true,
            lowerbodyVisible = true,
            feetVisible = true
        });
    }
}