using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface ICatalogBtnView
{
    event Action OnCatalogButtonClick;
    event Action OnHideTooltip;
    event Action<BaseEventData, string> OnShowTooltip;

    void OnPointerClick(DCLAction_Trigger action);
    void OnPointerEnter(PointerEventData eventData);
    void OnPointerExit();
}

public class CatalogBtnView : MonoBehaviour, ICatalogBtnView
{
    public event Action OnCatalogButtonClick;
    public event Action<BaseEventData, string> OnShowTooltip;
    public event Action OnHideTooltip;

    [SerializeField] internal Button mainButton;
    [SerializeField] internal string tooltipText = "Open Catalog (C)";
    [SerializeField] internal EventTrigger catalogButtonEventTrigger;
    [SerializeField] internal InputAction_Trigger toggleCatalogInputAction;

    private DCLAction_Trigger dummyActionTrigger = new DCLAction_Trigger();

    private const string VIEW_PATH = "GodMode/CatalogBtnView";

    internal static CatalogBtnView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<CatalogBtnView>();
        view.gameObject.name = "_CatalogBtnView";

        return view;
    }

    private void Awake()
    {
        mainButton.onClick.AddListener(() => OnPointerClick(dummyActionTrigger));
        toggleCatalogInputAction.OnTriggered += OnPointerClick;
        BuilderInWorldUtils.ConfigureEventTrigger(catalogButtonEventTrigger, EventTriggerType.PointerEnter, (eventData) => OnPointerEnter((PointerEventData)eventData));
        BuilderInWorldUtils.ConfigureEventTrigger(catalogButtonEventTrigger, EventTriggerType.PointerExit, (eventData) => OnPointerExit());
    }

    private void OnDestroy()
    {
        mainButton.onClick.RemoveAllListeners();
        toggleCatalogInputAction.OnTriggered -= OnPointerClick;
        BuilderInWorldUtils.RemoveEventTrigger(catalogButtonEventTrigger, EventTriggerType.PointerEnter);
        BuilderInWorldUtils.RemoveEventTrigger(catalogButtonEventTrigger, EventTriggerType.PointerExit);
    }

    public void OnPointerClick(DCLAction_Trigger action)
    {
        OnCatalogButtonClick?.Invoke();
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