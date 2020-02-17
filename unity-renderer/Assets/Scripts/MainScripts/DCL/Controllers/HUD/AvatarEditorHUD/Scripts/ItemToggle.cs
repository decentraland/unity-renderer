using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemToggle : UIButton, IPointerEnterHandler, IPointerExitHandler
{
    public event System.Action<ItemToggle> OnClicked;
    public event System.Action<ItemToggle> OnSellClicked;

    public WearableItem wearableItem { get; private set; }

    public Image thumbnail;
    public Image selectionHighlight;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] internal RectTransform amountContainer;
    [SerializeField] internal TextMeshProUGUI amountText;

    private bool selectedValue;

    private string loadedThumbnailURL;
    
    //Todo change this for a confirmation popup or implement it in a more elegant way
    public static Func<WearableItem, List<WearableItem>> getEquippedWearablesReplacedByFunc;

    public bool selected
    {
        get { return selectedValue; }
        set
        {
            selectedValue = value;
            SetSelection(selectedValue);
        }
    }

    protected virtual void SetSelection(bool isSelected)
    {
        if (selectionHighlight != null)
            selectionHighlight.enabled = isSelected;
    }

    protected new virtual void Awake()
    {
        base.Awake();

        warningPanel.SetActive(false);
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
        warningPanel.SetActive(false);
    }

    public virtual void Initialize(WearableItem w, bool isSelected, int amount)
    {
        ForgetThumbnail();
        wearableItem = w;
        selected = isSelected;
        amountContainer.gameObject.SetActive(amount > 1);
        amountText.text = $"x{amount.ToString()}";

        if(gameObject.activeInHierarchy)
            GetThumbnail();
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

    private void OnEnable()
    {
        GetThumbnail();
    }

    private void OnDisable()
    {
        ForgetThumbnail();
    }

    protected  virtual void OnDestroy()
    {
        OnClicked = null;
    }

    protected void CallOnSellClicked()
    {
        OnSellClicked?.Invoke(this);
    }
    
    private void GetThumbnail()
    {
        var url = wearableItem?.ComposeThumbnailUrl();

        ForgetThumbnail();

        if (wearableItem != null && !string.IsNullOrEmpty(url))
        {
            loadedThumbnailURL = url;
            ThumbnailsManager.GetThumbnail(url, OnThumbnailReady);
        }
    }
    
    private void ForgetThumbnail()
    {
        if(!string.IsNullOrEmpty(loadedThumbnailURL))
            ThumbnailsManager.ForgetThumbnail(loadedThumbnailURL, OnThumbnailReady);
        loadedThumbnailURL = null;
    }
}
