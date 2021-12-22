using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public interface IQuickBarView
{
    event Action<int> OnQuickBarInputTriggered;
    event Action<int> OnQuickBarObjectSelected;
    event Action<int, int, Texture> OnSceneObjectDroppedFromQuickBar;
    event Action<BaseEventData> OnSceneObjectDroppedFromCatalog;
    event Action<int> OnSetIndexToBeginDrag;
    event Action<int> OnSetIndexToDrop;

    void RaiseQuickBarInputTriggered(int index);
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

            BIWUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.BeginDrag, (eventData) =>
            {
                BeginDragSlot(triggerIndex);
            });

            BIWUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drag, (eventData) =>
            {
                DragSlot(eventData, triggerIndex);
            });

            BIWUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.EndDrag, (eventData) =>
            {
                EndDragSlot(triggerIndex);
            });

            BIWUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drop, (eventData) =>
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

        quickBar1InputAction.OnTriggered += OnQuickBar1InputTriggered;
        quickBar2InputAction.OnTriggered += OnQuickBar2InputTriggered;
        quickBar3InputAction.OnTriggered += OnQuickBar3InputTriggered;
        quickBar4InputAction.OnTriggered += OnQuickBar4InputTriggered;
        quickBar5InputAction.OnTriggered += OnQuickBar5InputTriggered;
        quickBar6InputAction.OnTriggered += OnQuickBar6InputTriggered;
        quickBar7InputAction.OnTriggered += OnQuickBar7InputTriggered;
        quickBar8InputAction.OnTriggered += OnQuickBar8InputTriggered;
        quickBar9InputAction.OnTriggered += OnQuickBar9InputTriggered;
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
            BIWUtils.RemoveEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drop);
        }

        // TODO(Brian): We should use an array here
        quickBar1InputAction.OnTriggered -= OnQuickBar1InputTriggered;
        quickBar2InputAction.OnTriggered -= OnQuickBar2InputTriggered;
        quickBar3InputAction.OnTriggered -= OnQuickBar3InputTriggered;
        quickBar4InputAction.OnTriggered -= OnQuickBar4InputTriggered;
        quickBar5InputAction.OnTriggered -= OnQuickBar5InputTriggered;
        quickBar6InputAction.OnTriggered -= OnQuickBar6InputTriggered;
        quickBar7InputAction.OnTriggered -= OnQuickBar7InputTriggered;
        quickBar8InputAction.OnTriggered -= OnQuickBar8InputTriggered;
        quickBar9InputAction.OnTriggered -= OnQuickBar9InputTriggered;

        if (draggedSlot != null)
            Object.Destroy(draggedSlot.gameObject);
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

    private void OnQuickBar1InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(0); }

    private void OnQuickBar2InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(1); }

    private void OnQuickBar3InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(2); }

    private void OnQuickBar4InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(3); }

    private void OnQuickBar5InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(4); }

    private void OnQuickBar6InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(5); }

    private void OnQuickBar7InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(6); }

    private void OnQuickBar8InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(7); }

    private void OnQuickBar9InputTriggered(DCLAction_Trigger action) { RaiseQuickBarInputTriggered(8); }

    public void RaiseQuickBarInputTriggered(int index) { OnQuickBarInputTriggered?.Invoke(index); }

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