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

        Avatar avatar = new Avatar(new WearableItemResolver(), new Loader(new WearableLoaderFactory(), gameObject), GetComponent<IAnimator>(), new Visibility(gameObject));

        yield return avatar.Load(avatarModel.wearables.Prepend(avatarModel.bodyShape).ToList(), new AvatarSettings
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