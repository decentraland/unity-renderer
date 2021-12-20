using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using GPUSkinning;
using UnityEngine;
using Avatar = AvatarSystem.Avatar;

//TODO REMOVE THIS TESTING CLASS, IF YOU SEE THIS IN A REVIEW, HIT ME
public class TestAvatarSystem : MonoBehaviour
{
    public TextAsset testAvatar;
    public TextAsset testAvatar2;
    public CancellationTokenSource cts = null;

    [ContextMenu("Load One")]
    public void LoadOne()
    {
        var avatar1 = JsonUtility.FromJson<AvatarModel>(testAvatar.text);
        Load(avatar1);
    }

    [ContextMenu("Repeat")]
    public void Repeat()
    {
        var avatar1 = JsonUtility.FromJson<AvatarModel>(testAvatar.text);
        var avatar2 = JsonUtility.FromJson<AvatarModel>(testAvatar2.text);

        Load(avatar1);
        Load(avatar2);
    }

    public void Load(AvatarModel avatarModel)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        MyLoad(avatarModel, cts.Token);
    }

    private Avatar avatar;
    private void Awake() { avatar = new Avatar(new AvatarCurator(new WearableItemResolver()), new Loader(new WearableLoaderFactory(), gameObject), GetComponent<IAnimator>(), new Visibility(gameObject), new NoLODs(), new SimpleGPUSkinning(), new GPUSkinningThrottler_New()); }

    public async UniTaskVoid MyLoad(AvatarModel avatarModel, CancellationToken ct)
    {
        await avatar.Load(avatarModel.wearables.Prepend(avatarModel.bodyShape).ToList(), new AvatarSettings
        {
            bodyshapeId = avatarModel.bodyShape,
            eyesColor = avatarModel.eyeColor,
            hairColor = avatarModel.hairColor,
            skinColor = avatarModel.skinColor
        }, ct);
    }
}