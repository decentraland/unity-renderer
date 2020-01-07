using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemToggle : UIButton, IPointerEnterHandler, IPointerExitHandler
{
    public event System.Action<ItemToggle> OnClicked;
    public WearableItem wearableItem { get; private set; }

    public Image thumbnail;
    public Image selectionHighlight;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] internal RectTransform amountContainer;
    [SerializeField] internal TextMeshProUGUI amountText;

    private bool selectedValue;

    //Todo change this for a confirmation popup or implement it in a more elegant way
    public static Func<WearableItem, List<WearableItem>> getEquippedWearablesReplacedByFunc;

    public bool selected
    {
        get { return selectedValue; }
        set
        {
            selectedValue = value;

            if (selectionHighlight != null)
                selectionHighlight.enabled = selectedValue;
        }
    }

    protected new virtual void Awake()
    {
        base.Awake();

        Application.quitting += Cleanup;
        warningPanel.SetActive(false);
    }

    void Cleanup()
    {
        OnClicked = null;
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
        warningPanel.SetActive(false);
    }

    public virtual void Initialize(WearableItem w, bool isSelected, int amount)
    {
        wearableItem = w;
        selected = isSelected;
        amountContainer.gameObject.SetActive(amount > 1);
        amountText.text = $"x{amount.ToString()}";

        if (!string.IsNullOrEmpty(w.thumbnail))
        {
            if (wearableItem != null)
            {
                ThumbnailsManager.CancelRequest(w.baseUrl + w.thumbnail, OnThumbnailReady);
            }
            ThumbnailsManager.RequestThumbnail(w.baseUrl + w.thumbnail, OnThumbnailReady);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        List<WearableItem> toReplace = getEquippedWearablesReplacedByFunc(wearableItem);
        if (wearableItem == null || toReplace.Count == 0) return;
        if (toReplace.Count == 1)
        {
            WearableItem w = toReplace[0];
            if (w.category == wearableItem.category) return;
        }

        warningPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        warningPanel.SetActive(false);
    }

    private void OnThumbnailReady(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }

    protected virtual void OnDestroy()
    {
        Application.quitting -= Cleanup;

        if (wearableItem != null)
        {
            ThumbnailsManager.CancelRequest(wearableItem.baseUrl + wearableItem.thumbnail, OnThumbnailReady);
        }
    }
}
