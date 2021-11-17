using System;
using System.Collections.Generic;

[Serializable]
public class ColumnsOrganizerComponentModel : BaseComponentModel
{
    public float spaceBetweenColumns;
    public List<ColumnModel> columns;
    public List<BaseComponentView> uiComponentsToRefreshSize;
}

[Serializable]
public class ColumnModel
{
    public bool isPercentage = false;
    public float width;
}