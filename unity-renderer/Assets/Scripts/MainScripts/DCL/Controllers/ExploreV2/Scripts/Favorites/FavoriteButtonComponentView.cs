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

    public override void Awake()
    {
        base.Awake();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SetFavoriteFromButton);
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetFavorite(model.isFavorite);
    }

    public bool IsFavorite() =>
        model?.isFavorite ?? false;

    private void SetFavoriteFromButton()
    {
        model.isFavorite = !model.isFavorite;
        OnFavoriteChange?.Invoke(model.placeUUID, model.isFavorite);
        RefreshControl();
    }

    private void SetFavorite(bool isFavorite)
    {
        SetButtonVisuals(isFavorite);
    }

    private void SetButtonVisuals(bool isFavorite) =>
        buttonFill.color = isFavorite ? favoriteFillColor : noFavoriteFillColor;

    public void Configure(FavoriteButtonComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }
}
