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
    private RectTransform rectTransform;

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    
    public void SetContent(int cols, int rows, List<LandWithAccess>lands)
    {
        contentPanelTransform.gameObject.SetActive( lands.Count > 0);
        projectCols = cols;
        projectRows = rows;
        selectedSet = false;

        SetContent(lands);
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

    public void HideEmptyContent()
    {
        emptyContentMark.SetActive(false);
    }

    internal void SelectedLand(LandWithAccess land)
    {
        foreach (PubllishLandListAdapter adapter in adapterList)
        {
            if (adapter.GetLand() != land)
                continue;

            if (adapter.GetState() == PubllishLandListAdapter.AdapterState.ENABLE)
            {
                OnLandSelected?.Invoke(land);
            }
            else
            {
                OnWrongLandSelected?.Invoke(land);
            }
        }
    }

    internal void CreateAdapter(LandWithAccess land)
    {
        Vector2Int rowsAndColum = BIWUtils.GetRowsAndColumsFromLand(land);
        PubllishLandListAdapter instanciatedAdapter = Instantiate(adapter, contentPanelTransform).GetComponent<PubllishLandListAdapter>();
        var status = rowsAndColum.x >= projectCols && rowsAndColum.y >= projectRows && BIWUtils.HasSquareSize(land) ? PubllishLandListAdapter.AdapterState.ENABLE : PubllishLandListAdapter.AdapterState.DISABLE;

        instanciatedAdapter.SetContent(land, status);
        instanciatedAdapter.OnLandSelected += SelectedLand;
        adapterList.Add(instanciatedAdapter);
    }
    
    private void Update() { HideIfClickedOutside(); }
    
    private void HideIfClickedOutside()
    {
        if (Input.GetMouseButtonDown(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            SetActive(false);
        }
    }
}