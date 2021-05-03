using System;
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

    private readonly ISectionSearchHandler sceneSearchHandler = new SectionSearchHandler();
    private Dictionary<string, ISceneCardView> scenesViews;

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

    void IDeployedSceneListener.OnSetScenes(Dictionary<string, ISceneCardView> scenes)
    {
        scenesViews = new Dictionary<string, ISceneCardView>(scenes);
        sceneSearchHandler.SetSearchableList(scenes.Values.Select(scene => scene.searchInfo).ToList());
    }

    void IDeployedSceneListener.OnSceneAdded(ISceneCardView scene)
    {
        scenesViews.Add(scene.sceneData.id, scene);
        sceneSearchHandler.AddItem(scene.searchInfo);
    }

    void IDeployedSceneListener.OnSceneRemoved(ISceneCardView scene)
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
                iterator.Current.Value.SetParent(view.GetCardsContainer());
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

        if (scenesViews.Count == 0)
        {
            if (isLoading)
            {
                view.SetLoading();
            }
            else
            {
                view.SetEmpty();
            }            
        }
        else if (searchInfoScenes.Count == 0)
        {
            view.SetNoSearchResult();
        }
        else
        {
            view.SetFilled();
        }
    }
}
