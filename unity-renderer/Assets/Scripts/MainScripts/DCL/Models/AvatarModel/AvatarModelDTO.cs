using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AvatarModelDTO
{
    public string id;
    public string name;
    public string bodyShape;
    public Color skinColor;
    public Color hairColor;
    public Color eyeColor;
    public List<string> wearables = new List<string>();
    public List<string> forceRender = new List<string>();

    public List<AvatarModel.AvatarEmoteEntry> emotes = new List<AvatarModel.AvatarEmoteEntry>();

    public string expressionTriggerId = null;
    public long expressionTriggerTimestamp = -1;
    public bool talking = false;

    public AvatarModel ToAvatarModel()
    {
        AvatarModel avatarModel = new AvatarModel
        {
            id = this.id,
            name = this.name,
            bodyShape = this.bodyShape,
            skinColor = this.skinColor,
            hairColor = this.hairColor,
            eyeColor = this.eyeColor,
            wearables = this.wearables,
            forceRender = new HashSet<string>(this.forceRender),
            emotes = this.emotes,
            expressionTriggerId = this.expressionTriggerId,
            expressionTriggerTimestamp = this.expressionTriggerTimestamp,
            talking = this.talking,
        };

        return avatarModel;
    }

    public static AvatarModelDTO FromAvatarModel(AvatarModel model)
    {
        AvatarModelDTO avatarModelDto = new AvatarModelDTO
        {
            id = model.id,
            name = model.name,
            bodyShape = model.bodyShape,
            skinColor = model.skinColor,
            hairColor = model.hairColor,
            eyeColor = model.eyeColor,
            wearables = model.wearables,
            forceRender = model.forceRender.ToList(),
            emotes = model.emotes,
            expressionTriggerId = model.expressionTriggerId,
            expressionTriggerTimestamp = model.expressionTriggerTimestamp,
            talking = model.talking,
        };

        return avatarModelDto;
    }
}
