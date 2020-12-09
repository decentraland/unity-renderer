using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionListview : ListView<BuildInWorldCompleteAction>
{
    public ActionAdapter adapter;

    public System.Action<BuildInWorldCompleteAction> OnActionSelected;


    List<ActionAdapter> actionList = new List<ActionAdapter>();
    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (BuildInWorldCompleteAction action in contentList)
        {
            CreateAdapter(action);
        }
    }

    public void ActionSelected(BuildInWorldCompleteAction action, ActionAdapter adapter)
    {
        OnActionSelected?.Invoke(action);
    }

    public override void RemoveAdapters()
    {
        base.RemoveAdapters();
        actionList.Clear();
    }

    public void AddAdapter(BuildInWorldCompleteAction action)
    {
        if (contentList == null)
            contentList = new List<BuildInWorldCompleteAction>();
        contentList.Add(action);
        CreateAdapter(action);
    }

    public void RefreshInfo()
    {
        foreach(ActionAdapter adapter in actionList)
        {
            adapter.RefreshIsDone();
        }
    }

    void CreateAdapter(BuildInWorldCompleteAction action)
    {
        ActionAdapter instanciatedAdapter = Instantiate(adapter, contentPanelTransform).GetComponent<ActionAdapter>();
        instanciatedAdapter.SetContent(action);
        instanciatedAdapter.OnActionSelected += ActionSelected;
        actionList.Add(instanciatedAdapter);
    }
}
