using System;
using UnityEngine.EventSystems;

public interface ICatalogBtnController
{
    event Action OnClick;

    void Initialize(ICatalogBtnView catalogBtnView, ITooltipController tooltipController);
    void Dispose();
    void Click();
    void ShowTooltip(BaseEventData eventData, string tooltipText);
    void HideTooltip();
}

public class CatalogBtnController : ICatalogBtnController
{
    public event Action OnClick;

    internal ICatalogBtnView catalogBtnView;
    internal ITooltipController tooltipController;

    public void Initialize(ICatalogBtnView catalogBtnView, ITooltipController tooltipController)
    {
        this.catalogBtnView = catalogBtnView;
        this.tooltipController = tooltipController;

        catalogBtnView.OnCatalogButtonClick += Click;
        catalogBtnView.OnShowTooltip += ShowTooltip;
        catalogBtnView.OnHideTooltip += HideTooltip;
    }

    public void Dispose()
    {
        catalogBtnView.OnCatalogButtonClick -= Click;
        catalogBtnView.OnShowTooltip -= ShowTooltip;
        catalogBtnView.OnHideTooltip -= HideTooltip;
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