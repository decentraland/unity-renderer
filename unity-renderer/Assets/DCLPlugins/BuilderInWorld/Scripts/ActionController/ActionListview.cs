using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[ExcludeFromCodeCoverage]
public class ActionListview : ListView<BIWCompleteAction>
{
    public ActionAdapter adapter;

    public System.Action<BIWCompleteAction> OnActionSelected;

    List<ActionAdapter> actionList = new List<ActionAdapter>();
    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (BIWCompleteAction action in contentList)
        {
            CreateAdapter(action);
        }
    }

    public void ActionSelected(BIWCompleteAction action, ActionAdapter adapter) { OnActionSelected?.Invoke(action); }

    public override void RemoveAdapters()
    {
        base.RemoveAdapters();
        actionList.Clear();
    }

    public void AddAdapter(BIWCompleteAction action)
    {
        if (contentList == null)
            contentList = new List<BIWCompleteAction>();
        contentList.Add(action);
        CreateAdapter(action);
    }

    public void RefreshInfo()
    {
        foreach (ActionAdapter adapter in actionList)
        {
            adapter.RefreshIsDone();
        }
    }

    void CreateAdapter(BIWCompleteAction action)
    {
        ActionAdapter instanciatedAdapter = Instantiate(adapter, contentPanelTransform).GetComponent<ActionAdapter>();
        instanciatedAdapter.SetContent(action);
        instanciatedAdapter.OnActionSelected += ActionSelected;
        actionList.Add(instanciatedAdapter);
    }
}