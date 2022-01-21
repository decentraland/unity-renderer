using System;
using System.Collections.Generic;

[Serializable]
public class DropdownComponentModel : BaseComponentModel
{
    public string title;
    public List<ToggleComponentModel> options;
}
