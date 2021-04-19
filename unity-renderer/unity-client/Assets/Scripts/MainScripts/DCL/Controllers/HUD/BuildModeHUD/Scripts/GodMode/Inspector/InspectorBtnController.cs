using System;
using UnityEngine.EventSystems;

public interface IInspectorBtnController
{
    event Action OnClick;

    void Initialize(IInspectorBtnView inspectorBtnView, ITooltipController tooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
}

public class InspectorBtnController : IInspectorBtnController
{
    public event Action OnClick;

    internal IInspectorBtnView inspectorBtnView;
    internal ITooltipController tooltipController;

    public void Initialize(IInspectorBtnView inspectorBtnView, ITooltipController tooltipController)
    {
        this.inspectorBtnView = inspectorBtnView;
        this.tooltipController = tooltipController;

        inspectorBtnView.OnInspectorButtonClick += Click;
        inspectorBtnView.OnShowTooltip += ShowTooltip;
        inspectorBtnView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        inspectorBtnView.OnInspectorButtonClick -= Click;
        inspectorBtnView.OnShowTooltip -= ShowTooltip;
        inspectorBtnView.OnHideTooltip -= HideTooltip;
    }

    public void Click()
    {
        OnClick?.Invoke();
    }

    public void ShowTooltip(BaseEventData eventData, string tooltipText)
    {
        tooltipController.ShowTooltip(eventData);
        tooltipController.SetTooltipText(tooltipText);
    }

    public void HideTooltip()
    {
        tooltipController.HideTooltip();
    }
}
