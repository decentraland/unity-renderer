using System;
using System.Collections.Generic;

[Serializable]
public class ColumnsOrganizerComponentModel : BaseComponentModel
{
    public float spaceBetweenColumns;
    public List<ColumnConfigModel> columnsConfig;
    public List<BaseComponentView> uiComponentsToRefreshSize;
}

[Serializable]
public class ColumnConfigModel
{
    public bool isPercentage = false;
    public float width;
}