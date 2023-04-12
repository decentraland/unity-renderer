using System;

[Serializable]
public class AvatarSlotComponentModel : BaseComponentModel
{
    public string rarity;
    public string imageUri;
    public string category;
    public bool isHidden;
    public string hiddenBy;
}
