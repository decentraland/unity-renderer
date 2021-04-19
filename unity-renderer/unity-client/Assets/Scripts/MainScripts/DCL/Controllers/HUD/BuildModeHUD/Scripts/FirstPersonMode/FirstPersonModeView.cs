using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IFirstPersonModeView
{
    event Action OnFirstPersonModeClick;
    event Action OnHideTooltip;
    event Action<BaseEventData, string> OnShowTooltip;

    void OnPointerClick();
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
}

public class FirstPersonModeView : MonoBehaviour, IFirstPersonModeView
{
    public event Action OnFirstPersonModeClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Change Camera (I)";
    [SerializeField] internal EventTrigger changeModeEventTrigger;

    private const string VIEW_PATH = "FirstPersonMode/FirstPersonModeView";

    internal static FirstPersonModeView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<FirstPersonModeView>();
        view.gameObject.name = "_FirstPersonModeView";

        return view;
    }

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        BuilderInWorldUtils.ConfigureEventTrigger(changeModeEventTrigger, EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        BuilderInWorldUtils.ConfigureEventTrigger(changeModeEventTrigger, EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveListener(OnPointerClick);
        BuilderInWorldUtils.RemoveEventTrigger(changeModeEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(changeModeEventTrigger, EventTriggerType.PointerExit);
    }

    public void OnPointerClick()
    {
        OnFirstPersonModeClick?.Invoke();
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
