using System;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;
using Object = UnityEngine.Object;

internal interface IScenesViewController : IDisposable
{
    event Action<Dictionary<string, ISceneCardView>> OnScenesSet;
    event Action<ISceneCardView> OnSceneAdded;
    event Action<ISceneCardView> OnSceneRemoved;
    event Action<Dictionary<string, ISceneCardView>> OnProjectScenesSet;
    event Action<ISceneCardView> OnProjectSceneAdded;
    event Action<ISceneCardView> OnProjectSceneRemoved;
    event Action<ISceneCardView> OnProjectSelected;
    event Action<Vector2Int> OnJumpInPressed;
    event Action<Vector2Int> OnEditorPressed;
    event Action<ISceneData, ISceneCardView> OnContextMenuPressed;
    event Action<string> OnRequestOpenUrl;
    void SetScenes(ISceneData[] scenesData);
    void SelectScene(string id);
    void AddListener(ISceneListener listener);
    void AddListener(ISelectSceneListener listener);
    void RemoveListener(ISceneListener listener);
    void RemoveListener(ISelectSceneListener listener);
}

/// <summary>
/// This class is responsible for receiving a list of scenes and merge it with the previous list
/// discriminating deployed and project scenes and triggering events when a new set of scenes arrive or
/// when new scenes are added or removed.
/// It instantiate and hold all the SceneCardViews to make them re-utilizable in every menu section screen.
/// </summary>
internal class ScenesViewController : IScenesViewController
{
    public event Action<Dictionary<string, ISceneCardView>> OnScenesSet;
    public event Action<ISceneCardView> OnSceneAdded;
    public event Action<ISceneCardView> OnSceneRemoved;
    public event Action<Dictionary<string, ISceneCardView>> OnProjectScenesSet;
    public event Action<ISceneCardView> OnProjectSceneAdded;
    public event Action<ISceneCardView> OnProjectSceneRemoved;
    public event Action<ISceneCardView> OnProjectSelected;
    public event Action<Vector2Int> OnJumpInPressed;
    public event Action<Vector2Int> OnEditorPressed;
    public event Action<ISceneData, ISceneCardView> OnContextMenuPressed;
    public event Action<string> OnRequestOpenUrl;

    private Dictionary<string, ISceneCardView> scenes = new Dictionary<string, ISceneCardView>();
    private Dictionary<string, ISceneCardView> projects = new Dictionary<string, ISceneCardView>();

    private ISceneCardView selectedScene;

    private readonly ScenesRefreshHelper scenesRefreshHelper = new ScenesRefreshHelper();
    private readonly SceneCardView sceneCardViewPrefab;
    private readonly Transform defaultParent;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="sceneCardViewPrefab">prefab for scene's card</param>
    /// <param name="defaultParent">default parent for scene's card</param>
    public ScenesViewController(SceneCardView sceneCardViewPrefab, Transform defaultParent = null)
    {
        this.sceneCardViewPrefab = sceneCardViewPrefab;
        this.defaultParent = defaultParent;
    }

    /// <summary>
    /// Set current user scenes (deployed and projects)
    /// </summary>
    /// <param name="scenesData">list of scenes</param>
    void IScenesViewController.SetScenes(ISceneData[] scenesData)
    {
        scenesRefreshHelper.Set(scenes, projects);

        scenes = new Dictionary<string, ISceneCardView>();
        projects = new Dictionary<string, ISceneCardView>();

        // update or create new scenes view
        for (int i = 0; i < scenesData.Length; i++)
        {
            SetScene(scenesData[i]);
        }

        // remove old deployed scenes
        if (scenesRefreshHelper.oldDeployedScenes != null)
        {
            using (var iterator = scenesRefreshHelper.oldDeployedScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    OnSceneRemoved?.Invoke(iterator.Current.Value);
                    DestroyCardView(iterator.Current.Value);
                }
            }
        }

        // remove old project scenes
        if (scenesRefreshHelper.oldProjectsScenes != null)
        {
            using (var iterator = scenesRefreshHelper.oldProjectsScenes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    OnProjectSceneRemoved?.Invoke(iterator.Current.Value);
                    DestroyCardView(iterator.Current.Value);
                }
            }
        }

        // notify scenes set if needed
        if (scenesRefreshHelper.isOldDeployedScenesEmpty)
        {
            OnScenesSet?.Invoke(scenes);
        }

        if (scenesRefreshHelper.isOldProjectScenesEmpty)
        {
            OnProjectScenesSet?.Invoke(projects);
        }
    }

    /// <summary>
    /// Set selected scene
    /// </summary>
    /// <param name="id">scene id</param>
    void IScenesViewController.SelectScene(string id)
    {
        ISceneCardView sceneCardView = null;
        if (scenes.TryGetValue(id, out ISceneCardView deployedSceneCardView))
        {
            sceneCardView = deployedSceneCardView;
        }
        else if (projects.TryGetValue(id, out ISceneCardView projectSceneCardView))
        {
            sceneCardView = projectSceneCardView;
        }

        selectedScene = sceneCardView;

        if (sceneCardView != null)
        {
            OnProjectSelected?.Invoke(sceneCardView);
        }
    }

    void IScenesViewController.AddListener(ISceneListener listener)
    {
        OnSceneAdded += listener.SceneAdded;
        OnSceneRemoved += listener.SceneRemoved;
        OnScenesSet += listener.SetScenes;
        listener.SetScenes(scenes);
    }

    void IScenesViewController.AddListener(ISelectSceneListener listener)
    {
        OnProjectSelected += listener.OnSelectScene;
        listener.OnSelectScene(selectedScene);
    }

    void IScenesViewController.RemoveListener(ISceneListener listener)
    {
        OnSceneAdded -= listener.SceneAdded;
        OnSceneRemoved -= listener.SceneRemoved;
        OnScenesSet -= listener.SetScenes;
    }
    
    void IScenesViewController.RemoveListener(ISelectSceneListener listener) { OnProjectSelected -= listener.OnSelectScene; }

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

    private void SetScene(ISceneData sceneData)
    {
        bool shouldNotifyAdd = (sceneData.isDeployed && !scenesRefreshHelper.isOldDeployedScenesEmpty) ||
                               (!sceneData.isDeployed && !scenesRefreshHelper.isOldProjectScenesEmpty);

        if (scenesRefreshHelper.IsSceneDeployStatusChanged(sceneData))
        {
            ChangeSceneDeployStatus(sceneData, shouldNotifyAdd);
        }
        else if (scenesRefreshHelper.IsSceneUpdate(sceneData))
        {
            UpdateScene(sceneData);
        }
        else
        {
            CreateScene(sceneData, shouldNotifyAdd);
        }
    }

    private void ChangeSceneDeployStatus(ISceneData sceneData, bool shouldNotifyAdd)
    {
        var cardView = RemoveScene(sceneData, true);
        cardView.Setup(sceneData);
        AddScene(sceneData, cardView, shouldNotifyAdd);
    }

    private void UpdateScene(ISceneData sceneData)
    {
        var cardView = RemoveScene(sceneData, false);
        cardView.Setup(sceneData);
        AddScene(sceneData, cardView, false);
    }

    private void CreateScene(ISceneData sceneData, bool shouldNotifyAdd)
    {
        var cardView = CreateCardView();
        cardView.Setup(sceneData);
        AddScene(sceneData, cardView, shouldNotifyAdd);
    }

    private ISceneCardView RemoveScene(ISceneData sceneData, bool notify)
    {
        bool wasDeployed = scenesRefreshHelper.WasDeployedScene(sceneData);
        var dictionary = wasDeployed ? scenesRefreshHelper.oldDeployedScenes : scenesRefreshHelper.oldProjectsScenes;

        if (dictionary.TryGetValue(sceneData.id, out ISceneCardView sceneCardView))
        {
            dictionary.Remove(sceneData.id);

            if (notify)
            {
                if (wasDeployed)
                    OnSceneRemoved?.Invoke(sceneCardView);
                else
                    OnProjectSceneRemoved?.Invoke(sceneCardView);
            }

            return sceneCardView;
        }

        return null;
    }

    private void AddScene(ISceneData sceneData, ISceneCardView sceneCardView, bool notify)
    {
        var dictionary = sceneData.isDeployed ? scenes : projects;
        if (dictionary.ContainsKey(sceneData.id))
            return;
        dictionary.Add(sceneData.id, sceneCardView);

        if (notify)
        {
            if (sceneData.isDeployed)
                OnSceneAdded?.Invoke(sceneCardView);
            else
                OnProjectSceneAdded?.Invoke(sceneCardView);
        }
    }

    private ISceneCardView CreateCardView()
    {
        SceneCardView sceneCardView = Object.Instantiate(sceneCardViewPrefab);
        ISceneCardView view = sceneCardView;

        view.SetActive(false);
        view.ConfigureDefaultParent(defaultParent);
        view.SetToDefaultParent();

        view.OnEditorPressed += OnEditorPressed;
        view.OnContextMenuPressed += OnContextMenuPressed;
        view.OnJumpInPressed += OnJumpInPressed;
        view.OnSettingsPressed += OnSceneSettingsPressed;

        return view;
    }

    private void DestroyCardView(ISceneCardView sceneCardView)
    {
        // NOTE: there is actually no need to unsubscribe here, but, just in case...
        sceneCardView.OnEditorPressed -= OnEditorPressed;
        sceneCardView.OnContextMenuPressed -= OnContextMenuPressed;
        sceneCardView.OnJumpInPressed -= OnJumpInPressed;
        sceneCardView.OnSettingsPressed -= OnSceneSettingsPressed;

        sceneCardView.Dispose();
    }

    private void OnSceneSettingsPressed(ISceneData sceneData) { OnRequestOpenUrl?.Invoke($"https://builder.decentraland.org/scenes/{sceneData.projectId}"); }
}