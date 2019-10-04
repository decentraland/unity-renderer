using UnityEngine;
using UnityEngine.UI;

public class ItemToggle : UIButton
{
    public event System.Action<ItemToggle> OnClicked;
    public WearableItem wearableItem { get; private set; }

    public Image thumbnail;
    public Image selectionHighlight;

    private bool selectedValue;

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
    }

    protected override void OnClick()
    {
        OnClicked?.Invoke(this);
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

    private void OnThumbnailReady(Sprite sprite)
    {
        thumbnail.sprite = sprite;
    }
}