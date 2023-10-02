using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NavmapFilterComponentView : BaseComponentView, INavmapFilterComponentView
{
    [SerializeField] private Button filterButton;
    [SerializeField] private Button closeButtonArea;
    [SerializeField] private GameObject filtersContainer;

    [SerializeField] private Toggle favoritesToggle;
    [SerializeField] private Toggle poisToggle;
    [SerializeField] private Toggle friendsToggle;
    [SerializeField] private Toggle peopleToggle;

    public event Action<MapLayer, bool> OnFilterChanged;

    public override void RefreshControl()
    {
    }

    public override void Awake()
    {
        base.Awake();

        filterButton.onClick.RemoveAllListeners();
        filterButton.onClick.AddListener(OnFilterButtonClicked);
        closeButtonArea.onClick.RemoveAllListeners();
        closeButtonArea.onClick.AddListener(OnFilterButtonClicked);

        favoritesToggle.onValueChanged.RemoveAllListeners();
        poisToggle.onValueChanged.RemoveAllListeners();
        friendsToggle.onValueChanged.RemoveAllListeners();
        peopleToggle.onValueChanged.RemoveAllListeners();

        favoritesToggle.onValueChanged.AddListener((isOn) => OnFilterChanged?.Invoke(MapLayer.Favorites, isOn));
        poisToggle.onValueChanged.AddListener((isOn) => OnFilterChanged?.Invoke(MapLayer.ScenesOfInterest, isOn));
        friendsToggle.onValueChanged.AddListener((isOn) => OnFilterChanged?.Invoke(MapLayer.Friends, isOn));
        peopleToggle.onValueChanged.AddListener((isOn) =>
        {
            OnFilterChanged?.Invoke(MapLayer.HotUsersMarkers, isOn);
            OnFilterChanged?.Invoke(MapLayer.ColdUsersMarkers, isOn);
        });

        filtersContainer.SetActive(false);
    }

    private void OnFilterButtonClicked() =>
        filtersContainer.SetActive(!filtersContainer.activeInHierarchy);
}
