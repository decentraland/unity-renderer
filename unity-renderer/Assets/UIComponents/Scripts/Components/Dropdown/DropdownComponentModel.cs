using System;
using System.Collections.Generic;

[Serializable]
public class DropdownComponentModel : BaseComponentModel
{
    public string title;
    public string searchPlaceHolderText;
    public string searchNotFoundText;
    public bool isMultiselect = true;
    public bool showSelectAllOption = true;
    public bool isOptionsPanelHeightDynamic = false;
    public float maxValueForDynamicHeight = 500f;
    public string emptyContentText;
    public List<ToggleComponentModel> options;
}
