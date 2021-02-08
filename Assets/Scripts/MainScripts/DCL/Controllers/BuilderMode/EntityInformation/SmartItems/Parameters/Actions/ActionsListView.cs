using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsListView : ListView<SmartItemActionEvent>
{
    public SmartItemActionEventAdapter adapter;


    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (SmartItemActionEvent actionEvent in contentList)
        {
            SmartItemActionEventAdapter adapter = Instantiate(this.adapter, contentPanelTransform).GetComponent<SmartItemActionEventAdapter>();
            adapter.SetContent(actionEvent);
        }
    }

    public override void RemoveAdapters()
    {
        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            SmartItemActionEventAdapter toRemove = contentPanelTransform.transform.GetChild(i).gameObject.GetComponent<SmartItemActionEventAdapter>();
            Destroy(toRemove.gameObject);
        }
    }

    public void AddActionEventAdapter(List<DCLBuilderInWorldEntity> entityList)
    {
        SmartItemActionEvent actionEvent = new SmartItemActionEvent();
        actionEvent.entityList = entityList;

        contentList.Add(actionEvent);
        RefreshDisplay();
    }
}
