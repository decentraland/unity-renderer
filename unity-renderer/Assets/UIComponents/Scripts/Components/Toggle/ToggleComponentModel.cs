using System;

[Serializable]
public class ToggleComponentModel : BaseComponentModel
{
    public bool isOn;
    public string text;
    public string id;
    public bool isTextActive;
    public bool changeTextColorOnSelect = false;
}
