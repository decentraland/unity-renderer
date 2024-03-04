using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL.Backpack
{
    public record BackpackEditorHUDModel
    {
        public WearableItem bodyShape = new() { id = "NOT_LOADED" };
        public Dictionary<string, WearableItem> wearables = new ();
        public Color hairColor;
        public Color skinColor;
        public Color eyesColor;
        public HashSet<string> forceRender = new ();

        public AvatarModel ToAvatarModel(Dictionary<string, string> extendedWearableUrns = null)
        {
            List<string> wearables;

            if (extendedWearableUrns != null)
            {
                wearables = new List<string>();

                foreach (string w in this.wearables.Keys)
                    wearables.Add(extendedWearableUrns.TryGetValue(w, out string extendedUrn)
                        ? extendedUrn : w);
            }
            else
                wearables = this.wearables.Keys.ToList();

            return new AvatarModel
            {
                bodyShape = bodyShape.id,
                wearables = wearables,
                hairColor = hairColor,
                skinColor = skinColor,
                eyeColor = eyesColor,
                forceRender = new HashSet<string>(forceRender)
            };
        }

        public void Update(BackpackEditorHUDModel newModel)
        {
            wearables.Clear();
            foreach (var wearable in newModel.wearables)
                wearables.Add(wearable.Key, wearable.Value);

            bodyShape = newModel.bodyShape;
            hairColor = newModel.hairColor;
            skinColor = newModel.skinColor;
            eyesColor = newModel.eyesColor;
            forceRender = new HashSet<string>(newModel.forceRender);
        }
    }
}
