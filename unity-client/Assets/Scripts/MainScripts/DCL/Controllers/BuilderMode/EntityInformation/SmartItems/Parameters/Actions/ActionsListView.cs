using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionsListView : ListView<SmartItemActionEvent>
{
    public SmartItemActionEventAdapter adapter;
    public System.Action<SmartItemActionable> OnActionableRemove;

    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (SmartItemActionEvent actionEvent in contentList)
        {
            SmartItemActionEventAdapter adapter = Instantiate(this.adapter, contentPanelTransform).GetComponent<SmartItemActionEventAdapter>();
            adapter.OnActionableRemove += RemoveActionable;
            adapter.SetContent(actionEvent);
        }
    }

    public override void RemoveAdapters()
    {
        for (int i = 0; i < contentPanelTransform.transform.childCount; i++)
        {
            SmartItemActionEventAdapter toRemove = contentPanelTransform.transform.GetChild(i).gameObject.GetComponent<SmartItemActionEventAdapter>();
            RemoveAdapter(toRemove);
        }
    }

    private void RemoveAdapter(SmartItemActionEventAdapter adapter)
    {
        adapter.OnActionableRemove -= RemoveActionable;
        Destroy(adapter.gameObject);
    }

    public void AddActionEventAdapter(SmartItemActionEvent actionEvent)
    {
        contentList.Add(actionEvent);
        RefreshDisplay();
    }

    private void RemoveActionable(SmartItemActionEventAdapter actionable)
    {
        contentList.Remove(actionable.GetContent());
        OnActionableRemove?.Invoke(actionable.GetContent().smartItemActionable);
        RemoveAdapter(actionable);
    }
}
