using DCLServices.MapRendererV2.MapLayers;
using System;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

public class NavmapFilterComponentView : BaseComponentView, INavmapFilterComponentView
{
    [SerializeField] private Button filterButton;
    [SerializeField] private Button closeButtonArea;
    [SerializeField] private GameObject filtersContainer;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button daoButton;
    [SerializeField] private TooltipComponentView tooltip;

    [SerializeField] private Toggle favoritesToggle;
    [SerializeField] private Toggle poisToggle;
    [SerializeField] private Toggle friendsToggle;
    [SerializeField] private Toggle peopleToggle;

    public event Action<MapLayer, bool> OnFilterChanged;
    public event Action OnClickedDAO;

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
        infoButton.onClick.RemoveAllListeners();
        infoButton.onClick.AddListener(OnInfoButtonClicked);
        daoButton.onClick.RemoveAllListeners();
        daoButton.onClick.AddListener(() => OnClickedDAO?.Invoke());

        favoritesToggle.onValueChanged.RemoveAllListeners();
        poisToggle.onValueChanged.RemoveAllListeners();
        friendsToggle.onValueChanged.RemoveAllListeners();
        peopleToggle.onValueChanged.RemoveAllListeners();

        favoritesToggle.onValueChanged.AddListener((isOn) => OnFilterChanged?.Invoke(MapLayer.Favorites, isOn));
        poisToggle.onValueChanged.AddListener((isOn) => OnFilterChanged?.Invoke(MapLayer.ScenesOfInterest, isOn));
        friendsToggle.onValueChanged.AddListener((isOn) => OnFilterChanged?.Invoke(MapLayer.Friends, isOn));
        peopleToggle.onValueChanged.AddListener((isOn) =>
        {
            OnFilterChanged?.Invoke(MapLayer.ColdUsersMarkers, isOn);
            OnFilterChanged?.Invoke(MapLayer.HotUsersMarkers, isOn);
        });

        filtersContainer.SetActive(false);
    }

    private void OnInfoButtonClicked()
    {
        if (!tooltip.gameObject.activeSelf)
            tooltip.Show();
    }

    private void OnFilterButtonClicked() =>
        filtersContainer.SetActive(!filtersContainer.activeInHierarchy);

    public override void Dispose()
    {
        base.Dispose();

        filterButton.onClick.RemoveAllListeners();
        closeButtonArea.onClick.RemoveAllListeners();
        infoButton.onClick.RemoveAllListeners();
        daoButton.onClick.RemoveAllListeners();
        favoritesToggle.onValueChanged.RemoveAllListeners();
        poisToggle.onValueChanged.RemoveAllListeners();
        friendsToggle.onValueChanged.RemoveAllListeners();
        peopleToggle.onValueChanged.RemoveAllListeners();
    }
}
