using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PublishLandListView : ListView<LandWithAccess>
{
    public event Action<LandWithAccess> OnLandSelected;
    public event Action<LandWithAccess> OnWrongLandSelected;

    [SerializeField] internal PubllishLandListAdapter adapter;

    private List<PubllishLandListAdapter> adapterList = new List<PubllishLandListAdapter>();
    private int projectRows;
    private int projectCols;

    private bool selectedSet = false;

    public void SetContent(int cols, int rows, LandWithAccess[] lands)
    {
        projectCols = cols;
        projectRows = rows;
        selectedSet = false;

        SetContent(lands.ToList());
    }

    public override void AddAdapters()
    {
        base.AddAdapters();

        foreach (LandWithAccess landWithAccess in contentList)
        {
            CreateAdapter(landWithAccess);
        }
    }

    public override void RemoveAdapters()
    {
        base.RemoveAdapters();
        adapterList.Clear();
    }

    internal void SelectedLand(LandWithAccess land)
    {
        PubllishLandListAdapter newSelectedAdapter = null;
        PubllishLandListAdapter lastSelectedAdapter = null;
        foreach (PubllishLandListAdapter adapter in adapterList)
        {
            if (adapter.GetState() == PubllishLandListAdapter.AdapterState.SELECTED)
            {
                adapter.SetState(PubllishLandListAdapter.AdapterState.ENABLE);
                lastSelectedAdapter = adapter;
            }

            if (adapter.GetLand() == land)
            {
                if (adapter.GetState() == PubllishLandListAdapter.AdapterState.ENABLE)
                {
                    adapter.SetState(PubllishLandListAdapter.AdapterState.SELECTED);
                    newSelectedAdapter = adapter;
                    OnLandSelected?.Invoke(land);
                }
                else
                {
                    OnWrongLandSelected?.Invoke(land);
                }
            }
        }

        if (newSelectedAdapter == null)
            lastSelectedAdapter?.SetState(PubllishLandListAdapter.AdapterState.ENABLE);
    }

    internal void CreateAdapter(LandWithAccess land)
    {
        Vector2Int rowsAndColum = BIWUtils.GetRowsAndColumsFromLand(land);
        PubllishLandListAdapter instanciatedAdapter = Instantiate(adapter, contentPanelTransform).GetComponent<PubllishLandListAdapter>();
        var status = rowsAndColum.x >= projectCols && rowsAndColum.y >= projectRows && BIWUtils.HasSquareSize(land) ? PubllishLandListAdapter.AdapterState.ENABLE : PubllishLandListAdapter.AdapterState.DISABLE;
        if (status == PubllishLandListAdapter.AdapterState.ENABLE && !selectedSet)
        {
            status = PubllishLandListAdapter.AdapterState.SELECTED;
            selectedSet = true;
            OnLandSelected?.Invoke(land);
        }

        instanciatedAdapter.SetContent(land, status);
        instanciatedAdapter.OnLandSelected += SelectedLand;
        adapterList.Add(instanciatedAdapter);
    }
}