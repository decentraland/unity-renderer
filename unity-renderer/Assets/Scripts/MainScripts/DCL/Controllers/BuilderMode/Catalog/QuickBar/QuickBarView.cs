using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickBarView : MonoBehaviour
{
    public QuickBarSlot[] shortcutsImgs;
    public Canvas generalCanvas;
    public CatalogGroupListView catalogGroupListView;


    public event System.Action<int> OnQuickBarShortcutSelected;
    public event System.Action<CatalogItem, int> OnQuickBarAdd;

    [SerializeField] internal InputAction_Trigger quickBar1InputAction;
    [SerializeField] internal InputAction_Trigger quickBar2InputAction;
    [SerializeField] internal InputAction_Trigger quickBar3InputAction;
    [SerializeField] internal InputAction_Trigger quickBar4InputAction;
    [SerializeField] internal InputAction_Trigger quickBar5InputAction;
    [SerializeField] internal InputAction_Trigger quickBar6InputAction;
    [SerializeField] internal InputAction_Trigger quickBar7InputAction;
    [SerializeField] internal InputAction_Trigger quickBar8InputAction;
    [SerializeField] internal InputAction_Trigger quickBar9InputAction;


    int lastIndexDroped = -1;


    private void Start()
    {
        quickBar1InputAction.OnTriggered += OnQuickBar1InputTriggedered;
        quickBar2InputAction.OnTriggered += OnQuickBar2InputTriggedered;
        quickBar3InputAction.OnTriggered += OnQuickBar3InputTriggedered;
        quickBar4InputAction.OnTriggered += OnQuickBar4InputTriggedered;
        quickBar5InputAction.OnTriggered += OnQuickBar5InputTriggedered;
        quickBar6InputAction.OnTriggered += OnQuickBar6InputTriggedered;
        quickBar7InputAction.OnTriggered += OnQuickBar7InputTriggedered;
        quickBar8InputAction.OnTriggered += OnQuickBar8InputTriggedered;
        quickBar9InputAction.OnTriggered += OnQuickBar9InputTriggedered;
    }

    private void OnDestroy()
    {
        quickBar1InputAction.OnTriggered -= OnQuickBar1InputTriggedered;
        quickBar2InputAction.OnTriggered -= OnQuickBar2InputTriggedered;
        quickBar3InputAction.OnTriggered -= OnQuickBar3InputTriggedered;
        quickBar4InputAction.OnTriggered -= OnQuickBar4InputTriggedered;
        quickBar5InputAction.OnTriggered -= OnQuickBar5InputTriggedered;
        quickBar6InputAction.OnTriggered -= OnQuickBar6InputTriggedered;
        quickBar7InputAction.OnTriggered -= OnQuickBar7InputTriggedered;
        quickBar8InputAction.OnTriggered -= OnQuickBar8InputTriggedered;
        quickBar9InputAction.OnTriggered -= OnQuickBar9InputTriggedered;
    }

    public void SetIndexToDrop(int index)
    {
        lastIndexDroped = index;
    }

    public void SceneObjectDropped(BaseEventData data)
    {
        CatalogItemAdapter adapter = catalogGroupListView.GetLastCatalogItemDragged();
        CatalogItem catalogItem = adapter.GetContent();

        if (adapter.thumbnailImg.enabled)
        {
            Texture texture = adapter.thumbnailImg.texture;
            SetQuickBarShortcut(catalogItem, lastIndexDroped, texture);
        }
    }

    private void SetQuickBarShortcut(CatalogItem catalogItem, int index, Texture texture)
    {
        OnQuickBarAdd?.Invoke(catalogItem, index);
     
        if (index >= shortcutsImgs.Length)
            return;

        if(shortcutsImgs[index] != null && texture != null)
            shortcutsImgs[index].SetTexture(texture);
    }

    public void QuickBarInput(int quickBarSlot)
    {
        OnQuickBarShortcutSelected?.Invoke(quickBarSlot);
    }

    private void OnQuickBar9InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(8);
    }
    private void OnQuickBar8InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(7);
    }
    private void OnQuickBar7InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(6);
    }
    private void OnQuickBar6InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(5);
    }
    private void OnQuickBar5InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(4);
    }
    private void OnQuickBar4InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(3);
    }
    private void OnQuickBar3InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(2);
    }
    private void OnQuickBar2InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(1);
    }
    private void OnQuickBar1InputTriggedered(DCLAction_Trigger action)
    {
        QuickBarInput(0);
    }
}
