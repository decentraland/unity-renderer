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

    private readonly ISectionSearchHandler sceneSearchHandler = new SectionSearchHandler();
    private Dictionary<string, ISceneCardView> scenesViews;

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

    void IProjectSceneListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes)
    {
        scenesViews = new Dictionary<string, ISceneCardView>(scenes);
        sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
    }

    void IProjectSceneListener.OnSceneAdded(ISceneCardView scene)
    {
        scenesViews.Add(scene.sceneData.id, scene);
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IProjectSceneListener.OnSceneRemoved(ISceneCardView scene)
    {
        scenesViews.Remove(scene.sceneData.id);
        scene.SetActive(false);
    }

    private void OnSearchResult(List<ISearchInfo> searchInfoScenes)
    {
        if (scenesViews == null)
            return;

        using (var iterator = scenesViews.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetParent(view.scenesCardContainer);
                iterator.Current.Value.SetActive(false);
            }
        }

        for (int i = 0; i < searchInfoScenes.Count; i++)
        {
            if (!scenesViews.TryGetValue(searchInfoScenes[i].id, out ISceneCardView cardView))
                continue;
            
            cardView.SetActive(true);
            cardView.SetSiblingIndex(i);
        }
        view.ResetScrollRect();
    }
}
