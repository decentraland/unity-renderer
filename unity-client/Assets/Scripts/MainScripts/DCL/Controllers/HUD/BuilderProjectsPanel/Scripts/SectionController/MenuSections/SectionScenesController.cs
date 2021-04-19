using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionScenesController : SectionBase, IDeployedSceneListener, IProjectSceneListener, ISectionOpenSectionRequester
{
    public event Action<SectionsController.SectionId> OnRequestOpenSection;
    
    internal const int MAX_CARDS = 3;
    internal readonly SectionScenesView view;

    private bool hasScenes = false;

    public override ISectionSearchHandler searchHandler => hasScenes ? sceneSearchHandler : null;
    public override SearchBarConfig searchBarConfig => new SearchBarConfig()
    {
        showFilterContributor = false,
        showFilterOperator = false,
        showFilterOwner = false,
        showResultLabel = false
    };

    private readonly SceneSearchHandler sceneSearchHandler = new SceneSearchHandler();

    internal Dictionary<string, SceneCardView> deployedViews;
    internal Dictionary<string, SceneCardView> projectViews;
    private List<SceneSearchInfo> searchList = new List<SceneSearchInfo>();

    public SectionScenesController()
    {
        var prefab = Resources.Load<SectionScenesView>("BuilderProjectsPanelMenuSections/SectionScenesView");
        view = Object.Instantiate(prefab);

        view.btnProjectsViewAll.onClick.AddListener(()=> OnRequestOpenSection?.Invoke(SectionsController.SectionId.SCENES_PROJECT));
        view.btnInWorldViewAll.onClick.AddListener(()=> OnRequestOpenSection?.Invoke(SectionsController.SectionId.SCENES_DEPLOYED));

        sceneSearchHandler.OnResult += OnSearchResult;
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.transform.SetParent(viewContainer);
        view.transform.ResetLocalTRS();
    }

    public override void Dispose()
    {
        Object.Destroy(view.gameObject);
    }

    protected override void OnShow()
    {
        view.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        view.gameObject.SetActive(false);
        searchList.Clear();
        deployedViews.Clear();
        projectViews.Clear();
    }

    private void ViewDirty()
    {
        bool hasDeployedScenes = view.deployedSceneContainer.childCount > 0;
        bool hasProjectScenes = view.projectSceneContainer.childCount > 0;
        hasScenes = hasDeployedScenes || hasProjectScenes;

        view.contentScreen.SetActive(hasScenes);
        view.emptyScreen.SetActive(!hasScenes);
        view.inWorldContainer.SetActive(hasDeployedScenes);
        view.projectsContainer.SetActive(hasProjectScenes);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        UpdateDictionary(ref deployedViews, scenes);
        searchList.AddRange(scenes.Values.Select(scene => scene.searchInfo));
        sceneSearchHandler.SetSearchableList(searchList);
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        UpdateDictionary(ref projectViews, scenes);
        searchList.AddRange(scenes.Values.Select(scene => scene.searchInfo));
        sceneSearchHandler.SetSearchableList(searchList);
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        deployedViews.Add(scene.sceneData.id, scene);
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        projectViews.Add(scene.sceneData.id, scene);
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        scene.SetParent(null);
        deployedViews.Remove(scene.sceneData.id);
        sceneSearchHandler.RemoveItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        scene.SetParent(null);
        projectViews.Remove(scene.sceneData.id);
        sceneSearchHandler.RemoveItem(scene.searchInfo);
    }

    private void OnSearchResult(List<SceneSearchInfo> searchInfoScenes)
    {
        if (deployedViews != null)
            SetResult(deployedViews, searchInfoScenes, view.deployedSceneContainer);

        if (projectViews != null)
            SetResult(projectViews, searchInfoScenes, view.projectSceneContainer);

        ViewDirty();
    }

    private void SetResult(Dictionary<string, SceneCardView> scenesViews, List<SceneSearchInfo> searchInfoScenes,
        Transform parent)
    {
        int count = 0;

        for (int i = 0; i < searchInfoScenes.Count; i++)
        {
            if (!scenesViews.TryGetValue(searchInfoScenes[i].id, out SceneCardView sceneView))
            {
                continue;
            }

            sceneView.SetParent(parent);
            sceneView.transform.SetSiblingIndex(count);
            sceneView.gameObject.SetActive(false);
            count++;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(i < count && i < MAX_CARDS);
        }
    }

    private void UpdateDictionary(ref Dictionary<string, SceneCardView> target, Dictionary<string, SceneCardView> newData)
    {
        if (newData.Count == 0)
            return;

        if (target == null)
        {
            target = new Dictionary<string, SceneCardView>(newData);
            return;
        }

        using (var iterator = newData.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (target.ContainsKey(iterator.Current.Key))
                    continue;

                target.Add(iterator.Current.Key, iterator.Current.Value);
            }
        }
    }
}
