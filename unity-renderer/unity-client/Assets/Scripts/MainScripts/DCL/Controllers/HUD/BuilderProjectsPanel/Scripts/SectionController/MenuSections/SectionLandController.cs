using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionLandController : SectionBase, ILandsListener
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionLandsView";

    public override ISectionSearchHandler searchHandler => landSearchHandler;

    private readonly SectionLandView view;

    private readonly LandSearchHandler landSearchHandler = new LandSearchHandler();
    private readonly Dictionary<string, LandElementView> landElementViews = new Dictionary<string, LandElementView>();
    private readonly Queue<LandElementView> landElementViewsPool = new Queue<LandElementView>();

    public SectionLandController() : this(
        Object.Instantiate(Resources.Load<SectionLandView>(VIEW_PREFAB_PATH))
    ) { }

    public SectionLandController(SectionLandView view)
    {
        this.view = view;
        PoolView(view.GetLandElementeBaseView());

        landSearchHandler.OnResult += OnSearchResult;
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    public override void Dispose()
    {
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

    void ILandsListener.OnSetLands(LandData[] lands)
    {
        view.SetEmpty(lands.Length == 0);

        List<LandElementView> toRemove = landElementViews.Values
                                                         .Where(landElementView => lands.All(land => land.id != landElementView.GetId()))
                                                         .ToList();

        for (int i = 0; i < toRemove.Count; i++)
        {
            landElementViews.Remove(toRemove[i].GetId());
            PoolView(toRemove[i]);
        }

        for (int i = 0; i < lands.Length; i++)
        {
            if (!landElementViews.TryGetValue(lands[i].id, out LandElementView landElementView))
            {
                landElementView = GetPooledView();
                landElementViews.Add(lands[i].id, landElementView);
            }

            landElementView.SetId(lands[i].id);
            landElementView.SetName(lands[i].name);
            landElementView.SetCoords(lands[i].x, lands[i].y);
            landElementView.SetSize(lands[i].size);
            landElementView.SetRole(lands[i].isOwner);
            landElementView.SetThumbnail(lands[i].thumbnailURL);
            landElementView.SetIsState(lands[i].isEstate);
        }
        landSearchHandler.SetSearchableList(landElementViews.Values.Select(scene => scene.searchInfo).ToList());
    }

    private void OnSearchResult(List<LandSearchInfo> searchInfoLands)
    {
        if (landElementViews == null)
            return;

        using (var iterator = landElementViews.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetParent(view.GetLandElementsContainer());
                iterator.Current.Value.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < searchInfoLands.Count; i++)
        {
            if (!landElementViews.TryGetValue(searchInfoLands[i].id, out LandElementView landView))
                continue;

            landView.gameObject.SetActive(true);
            landView.transform.SetSiblingIndex(i);
        }
        view.ResetScrollRect();
    }

    private void PoolView(LandElementView view)
    {
        view.SetActive(false);
        landElementViewsPool.Enqueue(view);
    }

    private LandElementView GetPooledView()
    {
        LandElementView landView;

        if (landElementViewsPool.Count > 0)
        {
            landView = landElementViewsPool.Dequeue();
        }
        else
        {
            landView = Object.Instantiate(view.GetLandElementeBaseView(), view.GetLandElementsContainer());
        }
        return landView;
    }
}