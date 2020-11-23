using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionListview : ListView<BuildModeAction>
{
    public ActionAdapter adapter;

    public System.Action<BuildModeAction> OnActionSelected;


    List<ActionAdapter> actionList = new List<ActionAdapter>();
    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (BuildModeAction action in contentList)
        {
            CreateAdapter(action);
        }
    }

    public void ActionSelected(BuildModeAction action, ActionAdapter adapter)
    {
        OnActionSelected?.Invoke(action);
    }

    public override void RemoveAdapters()
    {
        base.RemoveAdapters();
        actionList.Clear();
    }

    public void AddAdapter(BuildModeAction action)
    {
        if (contentList == null)
            contentList = new List<BuildModeAction>();
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

    void CreateAdapter(BuildModeAction action)
    {
        ActionAdapter instanciatedAdapter = Instantiate(adapter, contentPanelTransform).GetComponent<ActionAdapter>();
        instanciatedAdapter.SetContent(action);
        instanciatedAdapter.OnActionSelected += ActionSelected;
        actionList.Add(instanciatedAdapter);
    }
}
