using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AvatarModel
{
    public string id;
    public string name;
    public string bodyShape;
    public Color skinColor;
    public Color hairColor;
    public Color eyeColor;
    public List<string> wearables = new List<string>();
    public string expressionTriggerId = null;
    public long expressionTriggerTimestamp = -1;

    public void CopyFrom(AvatarModel other)
    {
        if (other == null) return;

        name = other.name;
        bodyShape = other.bodyShape;
        skinColor = other.skinColor;
        hairColor = other.hairColor;
        eyeColor = other.eyeColor;
        expressionTriggerId = other.expressionTriggerId;
        expressionTriggerTimestamp = other.expressionTriggerTimestamp;
        wearables = new List<string>(other.wearables);
    }
}
