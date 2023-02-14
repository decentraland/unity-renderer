using System;
using UnityEngine;
using UnityEngine.UI;

public class FavoriteButtonComponentView : BaseComponentView, IComponentModelConfig<FavoriteButtonComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] private ButtonComponentView button;
    [SerializeField] private Image buttonFill;
    [SerializeField] private Color noFavoriteFillColor;
    [SerializeField] private Color favoriteFillColor;

    [Header("Configuration")]
    [SerializeField] internal FavoriteButtonComponentModel model;

    public event Action<string, bool> OnFavoriteChange;

    private bool isFavorite;
    private string placeUUID;

    public override void Awake()
    {
        base.Awake();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SetFavorite);
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        isFavorite = model.isFavorite;
        SetFavorite();
    }

    private void SetFavorite()
    {
        isFavorite = !isFavorite;
        SetButtonVisuals(isFavorite);

        OnFavoriteChange?.Invoke(placeUUID, isFavorite);
    }

    private void SetButtonVisuals(bool isFavorite) =>
        buttonFill.color = isFavorite ? favoriteFillColor : noFavoriteFillColor;

    public void Configure(FavoriteButtonComponentModel newModel)
    {
        model = newModel;
        isFavorite = newModel.isFavorite;
        placeUUID = newModel.placeUUID;

        RefreshControl();
    }
}
