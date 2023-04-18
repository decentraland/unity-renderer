using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Backpack
{
    public record BackpackEditorHUDModel
    {
        public WearableItem bodyShape = new() { id = "NOT_LOADED" };
        public List<WearableItem> wearables = new ();
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
}
