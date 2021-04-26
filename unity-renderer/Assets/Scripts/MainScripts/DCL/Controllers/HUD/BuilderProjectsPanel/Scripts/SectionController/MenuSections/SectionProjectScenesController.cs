using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionProjectScenesController : SectionBase, IProjectSceneListener, ISectionHideContextMenuRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionProjectScenesView";
    
    public event Action OnRequestContextMenuHide;
    
    public override ISectionSearchHandler searchHandler => sceneSearchHandler;

    private readonly SectionProjectScenesView view;

    private readonly SceneSearchHandler sceneSearchHandler = new SceneSearchHandler();
    private Dictionary<string, SceneCardView> scenesViews;

    public SectionProjectScenesController() : this(
        Object.Instantiate(Resources.Load<SectionProjectScenesView>(VIEW_PREFAB_PATH))
        )
    {
    }
    
    public SectionProjectScenesController(SectionProjectScenesView view)
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
        Object.Destroy(view.gameObject);
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }

    void IProjectSceneListener.OnSetScenes(Dictionary<string, SceneCardView> scenes)
    {
        scenesViews = new Dictionary<string, SceneCardView>(scenes);
        sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
    }

    void IProjectSceneListener.OnSceneAdded(SceneCardView scene)
    {
        scenesViews.Add(scene.sceneData.id, scene);
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneRemoved(SceneCardView scene)
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
