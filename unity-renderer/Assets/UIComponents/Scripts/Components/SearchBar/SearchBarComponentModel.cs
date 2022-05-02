using System;

[Serializable]
public class SearchBarComponentModel : BaseComponentModel
{
    public float idleTimeToTriggerSearch = 1f;
    public string placeHolderText;
}
