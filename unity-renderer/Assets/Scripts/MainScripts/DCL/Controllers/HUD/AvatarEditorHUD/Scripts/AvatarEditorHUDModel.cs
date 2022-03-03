using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvatarEditorHUDModel
{
    public WearableItem bodyShape;
    public List<WearableItem> wearables = new List<WearableItem>();
    public Color hairColor;
    public Color skinColor;
    public Color eyesColor;

    public AvatarModel ToAvatarModel()
    {
        return new AvatarModel()
        {
            bodyShape = bodyShape.id,
            wearables = wearables.Select(x => x.id).ToList(),
            hairColor = hairColor,
            skinColor = skinColor,
            eyeColor = eyesColor
        };
    }
}