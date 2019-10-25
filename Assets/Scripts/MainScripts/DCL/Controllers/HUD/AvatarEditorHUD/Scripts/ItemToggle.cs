using System;
using System.Collections.Generic;
using System.Linq;
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

    private bool selectedValue;
    public static Func<WearableItem, List<string>> getWearablesReplacedByFunc;

    public bool selected
    {
        get { return selectedValue; }
        set
        {
            selectedValue = value;
            selectionHighlight.enabled = selectedValue;
        }
    }

    private new void Awake()
    {
        base.Awake();

        Application.quitting += () =>
        {
            OnClicked = null;
        };

        warningPanel.SetActive(false);
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
        warningPanel.SetActive(false);
    }

    public void Initialize(WearableItem w, bool isSelected)
    {
        if (wearableItem != null)
        {
            ThumbnailsManager.CancelRequest(w.baseUrl + w.thumbnail, OnThumbnailReady);
        }

        wearableItem = w;
        selected = isSelected;
        ThumbnailsManager.RequestThumbnail(w.baseUrl + w.thumbnail, OnThumbnailReady);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        List<string> toReplace = getWearablesReplacedByFunc(wearableItem);
        if (wearableItem == null || toReplace.Count == 0) return;
        if (toReplace.Count == 1)
        {
            WearableItem w = CatalogController.wearableCatalog.Get(toReplace[0]);
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
}