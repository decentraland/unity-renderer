using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IInspectorBtnView
{
    event Action OnHideTooltip;
    event Action OnInspectorButtonClick;
    event Action<BaseEventData, string> OnShowTooltip;

    void OnPointerClick(DCLAction_Trigger action);
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
}

public class InspectorBtnView : MonoBehaviour, IInspectorBtnView
{
    public event Action OnInspectorButtonClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Open Entity List (Q)";
    [SerializeField] internal EventTrigger inspectorButtonEventTrigger;
    [SerializeField] internal InputAction_Trigger toggleOpenEntityListInputAction;

    private DCLAction_Trigger dummyActionTrigger = new DCLAction_Trigger();

    private const string VIEW_PATH = "GodMode/Inspector/InspectorBtnView";

    internal static InspectorBtnView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<InspectorBtnView>();
        view.gameObject.name = "_InspectorBtnView";

        return view;
    }

    private void Awake()
    {
        mainButton.onClick.AddListener(() => OnPointerClick(dummyActionTrigger));
        toggleOpenEntityListInputAction.OnTriggered += OnPointerClick;
        BuilderInWorldUtils.ConfigureEventTrigger(inspectorButtonEventTrigger, EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        BuilderInWorldUtils.ConfigureEventTrigger(inspectorButtonEventTrigger, EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveAllListeners();
        toggleOpenEntityListInputAction.OnTriggered -= OnPointerClick;
        BuilderInWorldUtils.RemoveEventTrigger(inspectorButtonEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(inspectorButtonEventTrigger, EventTriggerType.PointerExit);
    }

    public void OnPointerClick(DCLAction_Trigger action)
    {
        OnInspectorButtonClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowTooltip?.Invoke(eventData, tooltipText);
    }

    public void OnPointerExit()
    {
        OnHideTooltip?.Invoke();
    }
}
