using System;
using System.Collections.Generic;

[Serializable]
public class DropdownComponentModel : BaseComponentModel
{
    public string title;
    public string searchPlaceHolderText;
    public bool isMultiselect = true;
    public bool showSelectAllOption = true;
    public bool isOptionsPanelHeightDynamic = false;
    public float maxValueForDynamicHeight = 500f;
    public List<ToggleComponentModel> options;
}
