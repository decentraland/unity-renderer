using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IQuickBarView
{
    event Action<int> OnQuickBarInputTriggered;
    event Action<int> OnQuickBarObjectSelected;
    event Action<int, int, Texture> OnSceneObjectDroppedFromQuickBar;
    event Action<BaseEventData> OnSceneObjectDroppedFromCatalog;
    event Action<int> OnSetIndexToBeginDrag;
    event Action<int> OnSetIndexToDrop;

    void OnQuickBarInputTriggedered(int index);
    void QuickBarObjectSelected(int index);
    void SceneObjectDroppedFromQuickBar(int fromIndex, int toIndex, Texture texture);
    void SceneObjectDroppedFromCatalog(BaseEventData data);
    void SetIndexToBeginDrag(int index);
    void SetIndexToDrop(int index);
    void SetTextureToShortcut(int shortcutIndex, Texture texture);
    void SetShortcutAsEmpty(int shortcutIndex);
    void BeginDragSlot(int triggerIndex);
    void DragSlot(BaseEventData eventData, int triggerIndex);
    void EndDragSlot(int triggerIndex);
    void CancelCurrentDragging();

}

public class QuickBarView : MonoBehaviour, IQuickBarView
{
    private const string VIEW_PATH = "Common/QuickBarView";

    public event Action<int> OnQuickBarObjectSelected;
    public event Action<int> OnSetIndexToBeginDrag;
    public event Action<int> OnSetIndexToDrop;
    public event Action<int, int, Texture> OnSceneObjectDroppedFromQuickBar;
    public event Action<BaseEventData> OnSceneObjectDroppedFromCatalog;
    public event Action<int> OnQuickBarInputTriggered;

    [SerializeField] internal Canvas generalCanvas;
    [SerializeField] internal QuickBarSlot[] shortcutsImgs;
    [SerializeField] internal Button[] shortcutsButtons;
    [SerializeField] internal EventTrigger[] shortcutsEventTriggers;
    [SerializeField] internal InputAction_Trigger quickBar1InputAction;
    [SerializeField] internal InputAction_Trigger quickBar2InputAction;
    [SerializeField] internal InputAction_Trigger quickBar3InputAction;
    [SerializeField] internal InputAction_Trigger quickBar4InputAction;
    [SerializeField] internal InputAction_Trigger quickBar5InputAction;
    [SerializeField] internal InputAction_Trigger quickBar6InputAction;
    [SerializeField] internal InputAction_Trigger quickBar7InputAction;
    [SerializeField] internal InputAction_Trigger quickBar8InputAction;
    [SerializeField] internal InputAction_Trigger quickBar9InputAction;

    internal int lastIndexToBeginDrag = -1;
    internal QuickBarSlot draggedSlot = null;

    internal static QuickBarView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuickBarView>();
        view.gameObject.name = "_QuickBarView";

        return view;
    }

    private void Awake()
    {
        CreateSlotToDrag();

        for (int i = 0; i < shortcutsButtons.Length; i++)
        {
            int buttonIndex = i;
            shortcutsButtons[buttonIndex].onClick.AddListener(() => QuickBarObjectSelected(buttonIndex));
        }

        for (int i = 0; i < shortcutsEventTriggers.Length; i++)
        {
            int triggerIndex = i;

            BuilderInWorldUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.BeginDrag, (eventData) =>
            {
                BeginDragSlot(triggerIndex);
            });

            BuilderInWorldUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drag, (eventData) =>
            {
                DragSlot(eventData, triggerIndex);
            });

            BuilderInWorldUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.EndDrag, (eventData) =>
            {
                EndDragSlot(triggerIndex);
            });

            BuilderInWorldUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drop, (eventData) =>
            {
                SetIndexToDrop(triggerIndex);

                if (lastIndexToBeginDrag != -1)
                {
                    SceneObjectDroppedFromQuickBar(lastIndexToBeginDrag, triggerIndex, shortcutsImgs[lastIndexToBeginDrag].image.texture);
                    CancelCurrentDragging();
                }
                else
                {
                    SceneObjectDroppedFromCatalog(eventData);
                }
            });
        }

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
        for (int i = 0; i < shortcutsButtons.Length; i++)
        {
            int buttonIndex = i;
            shortcutsButtons[buttonIndex].onClick.RemoveAllListeners();
        }

        for (int i = 0; i < shortcutsEventTriggers.Length; i++)
        {
            int triggerIndex = i;
            BuilderInWorldUtils.RemoveEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drop);
        }

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

    public void QuickBarObjectSelected(int index) { OnQuickBarObjectSelected?.Invoke(index); }

    public void SetIndexToBeginDrag(int index) { OnSetIndexToBeginDrag?.Invoke(index); }

    public void SetIndexToDrop(int index) { OnSetIndexToDrop?.Invoke(index); }

    public void SceneObjectDroppedFromQuickBar(int fromIndex, int toIndex, Texture texture) { OnSceneObjectDroppedFromQuickBar?.Invoke(fromIndex, toIndex, texture); }

    public void SceneObjectDroppedFromCatalog(BaseEventData data) { OnSceneObjectDroppedFromCatalog?.Invoke(data); }

    public void SetTextureToShortcut(int shortcutIndex, Texture texture)
    {
        if (shortcutIndex >= shortcutsImgs.Length)
            return;

        if (shortcutsImgs[shortcutIndex] != null && texture != null)
            shortcutsImgs[shortcutIndex].SetTexture(texture);
    }

    public void SetShortcutAsEmpty(int shortcutIndex)
    {
        if (shortcutIndex >= shortcutsImgs.Length)
            return;

        shortcutsImgs[shortcutIndex].SetEmpty();
    }

    private void OnQuickBar1InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(0); }

    private void OnQuickBar2InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(1); }

    private void OnQuickBar3InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(2); }

    private void OnQuickBar4InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(3); }

    private void OnQuickBar5InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(4); }

    private void OnQuickBar6InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(5); }

    private void OnQuickBar7InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(6); }

    private void OnQuickBar8InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(7); }

    private void OnQuickBar9InputTriggedered(DCLAction_Trigger action) { OnQuickBarInputTriggedered(8); }

    public void OnQuickBarInputTriggedered(int index) { OnQuickBarInputTriggered?.Invoke(index); }

    internal void CreateSlotToDrag()
    {
        if (shortcutsImgs.Length == 0)
            return;

        draggedSlot = Instantiate(shortcutsImgs[0], generalCanvas != null ? generalCanvas.transform : null);
        draggedSlot.EnableDragMode();
        draggedSlot.SetEmpty();
        draggedSlot.SetActive(false);
    }

    public void BeginDragSlot(int triggerIndex)
    {
        if (draggedSlot == null || shortcutsImgs[triggerIndex].isEmpty)
            return;

        lastIndexToBeginDrag = triggerIndex;
        SetIndexToBeginDrag(triggerIndex);
        draggedSlot.SetActive(true);
        draggedSlot.SetTexture(shortcutsImgs[triggerIndex].image.texture);
    }

    public void DragSlot(BaseEventData eventData, int triggerIndex)
    {
        if (draggedSlot == null || shortcutsImgs[triggerIndex].isEmpty)
            return;

        draggedSlot.slotTransform.position = ((PointerEventData)eventData).position;
    }

    public void EndDragSlot(int triggerIndex)
    {
        if (draggedSlot == null || shortcutsImgs[triggerIndex].isEmpty)
            return;

        draggedSlot.SetEmpty();
        draggedSlot.SetActive(false);
        QuickBarObjectSelected(triggerIndex);
    }

    public void CancelCurrentDragging()
    {
        lastIndexToBeginDrag = -1;

        if (draggedSlot != null)
        {
            draggedSlot.SetEmpty();
            draggedSlot.SetActive(false);
        }
    }
}