using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IQuickBarView
{
    event Action<int> OnQuickBarInputTriggered;
    event Action<int> OnQuickBarObjectSelected;
    event Action<BaseEventData> OnSceneObjectDropped;
    event Action<int> OnSetIndexToDrop;

    void OnQuickBarInputTriggedered(int index);
    void QuickBarObjectSelected(int index);
    void SceneObjectDropped(BaseEventData data);
    void SetIndexToDrop(int index);
    void SetTextureToShortcut(int shortcutIndex, Texture texture);
}

public class QuickBarView : MonoBehaviour, IQuickBarView
{
    public event Action<int> OnQuickBarObjectSelected;
    public event Action<int> OnSetIndexToDrop;
    public event Action<BaseEventData> OnSceneObjectDropped;
    public event Action<int> OnQuickBarInputTriggered;

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

    private const string VIEW_PATH = "Common/QuickBarView";

    internal static QuickBarView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<QuickBarView>();
        view.gameObject.name = "_QuickBarView";

        return view;
    }

    private void Awake()
    {
        for (int i = 0; i < shortcutsButtons.Length; i++)
        {
            int buttonIndex = i;
            shortcutsButtons[buttonIndex].onClick.AddListener(() => QuickBarObjectSelected(buttonIndex));
        }

        for (int i = 0; i < shortcutsEventTriggers.Length; i++)
        {
            int triggerIndex = i;
            BuilderInWorldUtils.ConfigureEventTrigger(shortcutsEventTriggers[triggerIndex], EventTriggerType.Drop, (eventData) =>
            {
                SetIndexToDrop(triggerIndex);
                SceneObjectDropped(eventData);
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

    public void QuickBarObjectSelected(int index)
    {
        OnQuickBarObjectSelected?.Invoke(index);
    }

    public void SetIndexToDrop(int index)
    {
        OnSetIndexToDrop?.Invoke(index);
    }

    public void SceneObjectDropped(BaseEventData data)
    {
        OnSceneObjectDropped?.Invoke(data);
    }

    public void SetTextureToShortcut(int shortcutIndex, Texture texture)
    {
        if (shortcutIndex >= shortcutsImgs.Length)
            return;

        if (shortcutsImgs[shortcutIndex] != null && texture != null)
            shortcutsImgs[shortcutIndex].SetTexture(texture);
    }

    private void OnQuickBar1InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(0);
    }

    private void OnQuickBar2InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(1);
    }

    private void OnQuickBar3InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(2);
    }

    private void OnQuickBar4InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(3);
    }

    private void OnQuickBar5InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(4);
    }

    private void OnQuickBar6InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(5);
    }

    private void OnQuickBar7InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(6);
    }

    private void OnQuickBar8InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(7);
    }

    private void OnQuickBar9InputTriggedered(DCLAction_Trigger action)
    {
        OnQuickBarInputTriggedered(8);
    }

    public void OnQuickBarInputTriggedered(int index)
    {
        OnQuickBarInputTriggered?.Invoke(index);
    }
}
