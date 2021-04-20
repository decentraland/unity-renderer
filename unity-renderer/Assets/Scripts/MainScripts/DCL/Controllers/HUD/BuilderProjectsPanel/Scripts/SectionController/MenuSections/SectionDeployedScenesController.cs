using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionDeployedScenesController : SectionBase, IDeployedSceneListener, ISectionHideContextMenuRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionDeployedScenesView";
    
    public event Action OnRequestContextMenuHide;
    
    public override ISectionSearchHandler searchHandler => sceneSearchHandler;

    private readonly SectionDeployedScenesView view;

    private readonly SceneSearchHandler sceneSearchHandler = new SceneSearchHandler();
    private Dictionary<string, SceneCardView> scenesViews;

    public SectionDeployedScenesController(): this(
        Object.Instantiate(Resources.Load<SectionDeployedScenesView>(VIEW_PREFAB_PATH))
    )
    {
    }
    
    public SectionDeployedScenesController(SectionDeployedScenesView view)
    {
        this.view = view;

        view.OnScrollRectValueChanged += OnRequestContextMenuHide;
        sceneSearchHandler.OnResult += OnSearchResult;
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    public override void Dispose()
    {
        view.OnScrollRectValueChanged -= OnRequestContextMenuHide;
        view.Dispose();
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        scenesViews = new Dictionary<string, SceneCardView>(scenes);
        sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
    }

    void IDeployedSceneListener.OnSceneAdded(SceneCardView scene)
    {
        scenesViews.Add(scene.sceneData.id, scene);
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IDeployedSceneListener.OnSceneRemoved(SceneCardView scene)
    {
        scenesViews.Remove(scene.sceneData.id);
        scene.gameObject.SetActive(false);
    }

    private void OnSearchResult(List<SceneSearchInfo> searchInfoScenes)
    {
        if (scenesViews == null)
            return;

        using (var iterator = scenesViews.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetParent(view.scenesCardContainer);
                iterator.Current.Value.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < searchInfoScenes.Count; i++)
        {
            if (!scenesViews.TryGetValue(searchInfoScenes[i].id, out SceneCardView cardView))
                continue;
            
            cardView.gameObject.SetActive(true);
            cardView.transform.SetSiblingIndex(i);
        }
        view.ResetScrollRect();
    }
}
