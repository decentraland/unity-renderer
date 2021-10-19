using System;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;
using Object = UnityEngine.Object;

internal interface IPlacesViewController : IDisposable
{
    event Action<Dictionary<string, IPlaceCardView>> OnPlacesSet;
    event Action<IPlaceCardView> OnPlaceAdded;
    event Action<IPlaceCardView> OnPlaceRemoved;
    event Action<Dictionary<string, IPlaceCardView>> OnProjectScenesSet;
    event Action<IPlaceCardView> OnProjectSceneAdded;
    event Action<IPlaceCardView> OnProjectSceneRemoved;
    event Action<IPlaceCardView> OnProjectSelected;
    event Action<Vector2Int> OnJumpInPressed;
    event Action<Vector2Int> OnEditorPressed;
    event Action<IPlaceData, IPlaceCardView> OnContextMenuPressed;
    event Action<string> OnRequestOpenUrl;
    void SetPlaces(IPlaceData[] scenesData);
    void SelectPlace(string id);
    void AddListener(IPlaceListener listener);
    void AddListener(IProjectListener listener);
    void AddListener(ISelectPlaceListener listener);
    void RemoveListener(IPlaceListener listener);
    void RemoveListener(IProjectListener listener);
    void RemoveListener(ISelectPlaceListener listener);
}

/// <summary>
/// This class is responsible for receiving a list of scenes and merge it with the previous list
/// discriminating deployed and project scenes and triggering events when a new set of scenes arrive or
/// when new scenes are added or removed.
/// It instantiate and hold all the SceneCardViews to make them re-utilizable in every menu section screen.
/// </summary>
internal class PlacesViewController : IPlacesViewController
{
    public event Action<Dictionary<string, IPlaceCardView>> OnPlacesSet;
    public event Action<IPlaceCardView> OnPlaceAdded;
    public event Action<IPlaceCardView> OnPlaceRemoved;
    public event Action<Dictionary<string, IPlaceCardView>> OnProjectScenesSet;
    public event Action<IPlaceCardView> OnProjectSceneAdded;
    public event Action<IPlaceCardView> OnProjectSceneRemoved;
    public event Action<IPlaceCardView> OnProjectSelected;
    public event Action<Vector2Int> OnJumpInPressed;
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<IPlaceData, IPlaceCardView> OnContextMenuPressed;
    public event Action<string> OnRequestOpenUrl;

    private Dictionary<string, IPlaceCardView> scenes = new Dictionary<string, IPlaceCardView>();
    private Dictionary<string, IPlaceCardView> projects = new Dictionary<string, IPlaceCardView>();

    private IPlaceCardView selectedPlace;

    private readonly PlacesRefreshHelper placesRefreshHelper = new PlacesRefreshHelper();
    private readonly PlaceCardView placeCardViewPrefab;
    private readonly Transform defaultParent;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="placeCardViewPrefab">prefab for scene's card</param>
    /// <param name="defaultParent">default parent for scene's card</param>
    public PlacesViewController(PlaceCardView placeCardViewPrefab, Transform defaultParent = null)
    {
        this.placeCardViewPrefab = placeCardViewPrefab;
        this.defaultParent = defaultParent;
    }

    /// <summary>
    /// Set current user scenes (deployed and projects)
    /// </summary>
    /// <param name="scenesData">list of scenes</param>
    void IPlacesViewController.SetPlaces(IPlaceData[] scenesData)
    {
        placesRefreshHelper.Set(scenes, projects);

        scenes = new Dictionary<string, IPlaceCardView>();
        projects = new Dictionary<string, IPlaceCardView>();

        // update or create new scenes view
        for (int i = 0; i < scenesData.Length; i++)
        {
            SetScene(scenesData[i]);
        }

        // remove old deployed scenes
        if (placesRefreshHelper.oldDeployedScenes != null)
        {
            using (var iterator = placesRefreshHelper.oldDeployedScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    OnPlaceRemoved?.Invoke(iterator.Current.Value);
                    DestroyCardView(iterator.Current.Value);
                }
            }
        }

        // remove old project scenes
        if (placesRefreshHelper.oldProjectsScenes != null)
        {
            using (var iterator = placesRefreshHelper.oldProjectsScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    OnProjectSceneRemoved?.Invoke(iterator.Current.Value);
                    DestroyCardView(iterator.Current.Value);
                }
            }
        }

        // notify scenes set if needed
        if (placesRefreshHelper.isOldDeployedScenesEmpty)
        {
            OnPlacesSet?.Invoke(scenes);
        }

        if (placesRefreshHelper.isOldProjectScenesEmpty)
        {
            OnProjectScenesSet?.Invoke(projects);
        }
    }

    /// <summary>
    /// Set selected scene
    /// </summary>
    /// <param name="id">scene id</param>
    void IPlacesViewController.SelectPlace(string id)
    {
        IPlaceCardView placeCardView = null;
        if (scenes.TryGetValue(id, out IPlaceCardView deployedSceneCardView))
        {
            placeCardView = deployedSceneCardView;
        }
        else if (projects.TryGetValue(id, out IPlaceCardView projectSceneCardView))
        {
            placeCardView = projectSceneCardView;
        }

        selectedPlace = placeCardView;

        if (placeCardView != null)
        {
            OnProjectSelected?.Invoke(placeCardView);
        }
    }

    void IPlacesViewController.AddListener(IPlaceListener listener)
    {
        OnPlaceAdded += listener.PlaceAdded;
        OnPlaceRemoved += listener.PlaceRemoved;
        OnPlacesSet += listener.SetPlaces;
        listener.SetPlaces(scenes);
    }

    void IPlacesViewController.AddListener(IProjectListener listener)
    {
        OnProjectSceneAdded += listener.PlaceAdded;
        OnProjectSceneRemoved += listener.PlaceRemoved;
        OnProjectScenesSet += listener.SetPlaces;
        listener.SetPlaces(projects);
    }

    void IPlacesViewController.AddListener(ISelectPlaceListener listener)
    {
        OnProjectSelected += listener.OnSelectScene;
        listener.OnSelectScene(selectedPlace);
    }

    void IPlacesViewController.RemoveListener(IPlaceListener listener)
    {
        OnPlaceAdded -= listener.PlaceAdded;
        OnPlaceRemoved -= listener.PlaceRemoved;
        OnPlacesSet -= listener.SetPlaces;
    }
    void IPlacesViewController.RemoveListener(IProjectListener listener)
    {
        OnProjectSceneAdded -= listener.PlaceAdded;
        OnProjectSceneRemoved -= listener.PlaceRemoved;
        OnProjectScenesSet -= listener.SetPlaces;
    }
    void IPlacesViewController.RemoveListener(ISelectPlaceListener listener) { OnProjectSelected -= listener.OnSelectScene; }

    public void Dispose()
    {
        using (var iterator = scenes.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Dispose();
            }
        }

        scenes.Clear();

        using (var iterator = projects.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Dispose();
            }
        }

        projects.Clear();
    }

    private void SetScene(IPlaceData placeData)
    {
        bool shouldNotifyAdd = (placeData.isDeployed && !placesRefreshHelper.isOldDeployedScenesEmpty) ||
                               (!placeData.isDeployed && !placesRefreshHelper.isOldProjectScenesEmpty);

        if (placesRefreshHelper.IsSceneDeployStatusChanged(placeData))
        {
            ChangeSceneDeployStatus(placeData, shouldNotifyAdd);
        }
        else if (placesRefreshHelper.IsSceneUpdate(placeData))
        {
            UpdateScene(placeData);
        }
        else
        {
            CreateScene(placeData, shouldNotifyAdd);
        }
    }

    private void ChangeSceneDeployStatus(IPlaceData placeData, bool shouldNotifyAdd)
    {
        var cardView = RemoveScene(placeData, true);
        cardView.Setup(placeData);
        AddScene(placeData, cardView, shouldNotifyAdd);
    }

    private void UpdateScene(IPlaceData placeData)
    {
        var cardView = RemoveScene(placeData, false);
        cardView.Setup(placeData);
        AddScene(placeData, cardView, false);
    }

    private void CreateScene(IPlaceData placeData, bool shouldNotifyAdd)
    {
        var cardView = CreateCardView();
        cardView.Setup(placeData);
        AddScene(placeData, cardView, shouldNotifyAdd);
    }

    private IPlaceCardView RemoveScene(IPlaceData placeData, bool notify)
    {
        bool wasDeployed = placesRefreshHelper.WasDeployedScene(placeData);
        var dictionary = wasDeployed ? placesRefreshHelper.oldDeployedScenes : placesRefreshHelper.oldProjectsScenes;

        if (dictionary.TryGetValue(placeData.id, out IPlaceCardView sceneCardView))
        {
            dictionary.Remove(placeData.id);

            if (notify)
            {
                if (wasDeployed)
                    OnPlaceRemoved?.Invoke(sceneCardView);
                else
                    OnProjectSceneRemoved?.Invoke(sceneCardView);
            }

            return sceneCardView;
        }

        return null;
    }

    private void AddScene(IPlaceData placeData, IPlaceCardView placeCardView, bool notify)
    {
        var dictionary = placeData.isDeployed ? scenes : projects;
        if (dictionary.ContainsKey(placeData.id))
            return;
        dictionary.Add(placeData.id, placeCardView);

        if (notify)
        {
            if (placeData.isDeployed)
                OnPlaceAdded?.Invoke(placeCardView);
            else
                OnProjectSceneAdded?.Invoke(placeCardView);
        }
    }

    private IPlaceCardView CreateCardView()
    {
        PlaceCardView placeCardView = Object.Instantiate(placeCardViewPrefab);
        IPlaceCardView view = placeCardView;

        view.SetActive(false);
        view.ConfigureDefaultParent(defaultParent);
        view.SetToDefaultParent();

        view.OnEditorPressed += OnEditorPressed;
        view.OnContextMenuPressed += OnContextMenuPressed;
        view.OnJumpInPressed += OnJumpInPressed;
        view.OnSettingsPressed += OnSceneSettingsPressed;

        return view;
    }

    private void DestroyCardView(IPlaceCardView placeCardView)
    {
        // NOTE: there is actually no need to unsubscribe here, but, just in case...
        placeCardView.OnEditorPressed -= OnEditorPressed;
        placeCardView.OnContextMenuPressed -= OnContextMenuPressed;
        placeCardView.OnJumpInPressed -= OnJumpInPressed;
        placeCardView.OnSettingsPressed -= OnSceneSettingsPressed;

        placeCardView.Dispose();
    }

    private void OnSceneSettingsPressed(IPlaceData placeData) { OnRequestOpenUrl?.Invoke($"https://builder.decentraland.org/scenes/{placeData.projectId}"); }
}