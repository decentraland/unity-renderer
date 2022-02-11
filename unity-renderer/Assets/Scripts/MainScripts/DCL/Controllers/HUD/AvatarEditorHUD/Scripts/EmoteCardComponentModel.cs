using System;
using UnityEngine;

[Serializable]
internal class EmoteCardComponentModel : BaseComponentModel
{
    public string id;
    public Sprite pictureSprite;
    public string pictureUri;
    public bool isEquipped = false;
    public bool isSelected = false;
    public int assignedSlot = -1;
}