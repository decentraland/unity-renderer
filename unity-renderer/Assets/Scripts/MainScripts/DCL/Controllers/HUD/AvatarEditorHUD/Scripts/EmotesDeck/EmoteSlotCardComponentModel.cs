using System;
using UnityEngine;

[Serializable]
internal class EmoteSlotCardComponentModel : BaseComponentModel
{
    public string id;
    public Sprite pictureSprite;
    public string pictureUri;
    public bool isSelected = false;
    public int slotNumber = -1;
}
