using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionLandController : SectionBase, ILandsListener, ISectionOpenURLRequester, ISectionGotoCoordsRequester, ISectionEditSceneAtCoordsRequester
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionLandsView";
    
    private const string BUILDER_LAND_URL_FORMAT = "https://builder.decentraland.org/land/{0}";
    private const string MARKETPLACE_URL = "https://market.decentraland.org/";

    public event Action<string> OnRequestOpenUrl;
    public event Action<Vector2Int> OnRequestGoToCoords;
    public event Action<Vector2Int> OnRequestEditSceneAtCoords;

    public override ISectionSearchHandler searchHandler => landSearchHandler;

    private readonly SectionLandView view;

    private readonly ISectionSearchHandler landSearchHandler = new SectionSearchHandler();
    private readonly Dictionary<string, LandElementView> landElementViews = new Dictionary<string, LandElementView>();
    private readonly Queue<LandElementView> landElementViewsPool = new Queue<LandElementView>();

    public SectionLandController() : this(
        Object.Instantiate(Resources.Load<SectionLandView>(VIEW_PREFAB_PATH))
    ) { }

    public SectionLandController(SectionLandView view)
    {
        this.view = view;
        PoolView(view.GetLandElementeBaseView());

        view.OnOpenMarketplaceRequested += OnOpenMarketplacePressed;
        landSearchHandler.OnResult += OnSearchResult;
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    public override void Dispose()
    {
        view.OnOpenMarketplaceRequested -= OnOpenMarketplacePressed;
        landSearchHandler.OnResult -= OnSearchResult;
        
        using (var iterator = landElementViews.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
               iterator.Dispose();
            }
        }

        while (landElementViewsPool.Count > 0)
        {
            landElementViewsPool.Dequeue().Dispose();
        }
        
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

    void ILandsListener.OnSetLands(LandWithAccess[] lands)
    {
        if (lands == null || lands.Length == 0)
        {
            SetEmptyOrLoading();
        }

        if (lands == null)
            return;

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

            bool isEstate = lands[i].type == LandType.ESTATE;
            landElementView.Setup(lands[i]);
            landElementView.SetThumbnail(GetLandThumbnailUrl(lands[i], isEstate));
        }
        landSearchHandler.SetSearchableList(landElementViews.Values.Select(scene => scene.searchInfo).ToList());
    }

    private void OnSearchResult(List<ISearchInfo> searchInfoLands)
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

        if (landElementViews.Count == 0)
        {
            SetEmptyOrLoading();
        }
        else if (searchInfoLands.Count == 0)
        {
            view.SetNoSearchResult();
        }
        else
        {
            view.SetFilled();
        }        
    }

    private void PoolView(LandElementView landView)
    {
        landView.OnSettingsPressed -= OnSettingsPressed;
        landView.OnJumpInPressed -= OnJumpInPressed;
        landView.OnOpenInDappPressed -= OnOpenInDappPressed;
        landView.OnEditorPressed -= OnEditPressed;
        
        landView.SetActive(false);
        landElementViewsPool.Enqueue(landView);
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
        
        landView.OnSettingsPressed += OnSettingsPressed;
        landView.OnJumpInPressed += OnJumpInPressed;
        landView.OnOpenInDappPressed += OnOpenInDappPressed;
        landView.OnEditorPressed += OnEditPressed;
        
        return landView;
    }

    private void SetEmptyOrLoading()
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

    private string GetLandThumbnailUrl(LandWithAccess land, bool isEstate)
    {
        if (land == null)
            return null;
        
        const int width = 100;
        const int height = 100;
        const int sizeFactorParcel = 15;
        const int sizeFactorEstate = 35;
        
        if (!isEstate)
        {
            return MapUtils.GetMarketPlaceThumbnailUrl(new[] { land.@base }, width, height, sizeFactorParcel);
        }

        return MapUtils.GetMarketPlaceThumbnailUrl(land.parcels, width, height, sizeFactorEstate);
    }

    private void OnSettingsPressed(string landId)
    {
        //NOTE: for MVP we are redirecting user to Builder's page
        WebInterface.OpenURL(string.Format(BUILDER_LAND_URL_FORMAT, landId));
    }

    private void OnOpenInDappPressed(string landId)
    {
        OnRequestOpenUrl?.Invoke(string.Format(BUILDER_LAND_URL_FORMAT, landId));
    }

    private void OnJumpInPressed(Vector2Int coords)
    {
        OnRequestGoToCoords?.Invoke(coords);
    }

    private void OnOpenMarketplacePressed()
    {
        OnRequestOpenUrl?.Invoke(MARKETPLACE_URL);
    }
    
    private void OnEditPressed(Vector2Int coords)
    {
        OnRequestEditSceneAtCoords?.Invoke(coords);
    }
}
