using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IPublishBtnView
{
    event Action OnHideTooltip;
    event Action OnPublishButtonClick;
    event Action<BaseEventData, string> OnShowTooltip;

    void OnPointerClick();
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
    void SetInteractable(bool isInteractable);
}

public class PublishBtnView : MonoBehaviour, IPublishBtnView
{
    public event Action OnPublishButtonClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Publish Scene";
    [SerializeField] internal EventTrigger publishButtonEventTrigger;

    private const string VIEW_PATH = "GodMode/PublishBtnView";

    internal static PublishBtnView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PublishBtnView>();
        view.gameObject.name = "_PublishBtnView";

        return view;
    }

    private void Awake()
    {
        mainButton.onClick.AddListener(OnPointerClick);
        BuilderInWorldUtils.ConfigureEventTrigger(publishButtonEventTrigger, EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        BuilderInWorldUtils.ConfigureEventTrigger(publishButtonEventTrigger, EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveListener(OnPointerClick);
        BuilderInWorldUtils.RemoveEventTrigger(publishButtonEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(publishButtonEventTrigger, EventTriggerType.PointerExit);
    }

    public void OnPointerClick()
    {
        OnPublishButtonClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowTooltip?.Invoke(eventData, tooltipText);
    }

    public void OnPointerExit()
    {
        OnHideTooltip?.Invoke();
    }

    public void SetInteractable(bool isInteractable)
    {
        mainButton.interactable = isInteractable;
    }
}
