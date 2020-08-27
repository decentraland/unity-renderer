using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;

internal class HighlightScenesController : MonoBehaviour
{
    const float SCENES_UPDATE_INTERVAL = 60;

    [SerializeField] HotSceneCellView hotsceneBaseCellView;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] GameObject loadingSpinner;

    Dictionary<Vector2Int, HotSceneCellView> cachedHotScenes = new Dictionary<Vector2Int, HotSceneCellView>();
    Dictionary<Vector2Int, BaseSceneCellView> activeHotSceneViews = new Dictionary<Vector2Int, BaseSceneCellView>();

    ExploreMiniMapDataController mapDataController;
    FriendTrackerController friendsController;

    ViewPool<HotSceneCellView> hotScenesViewPool;

    float lastTimeRefreshed = 0;

    public void Initialize(ExploreMiniMapDataController mapDataController, FriendTrackerController friendsController)
    {
        this.mapDataController = mapDataController;
        this.friendsController = friendsController;
        hotScenesViewPool = new ViewPool<HotSceneCellView>(hotsceneBaseCellView, 5);
    }

    public void RefreshIfNeeded()
    {
        if (cachedHotScenes.Count == 0 || HotScenesController.i.timeSinceLastUpdate >= SCENES_UPDATE_INTERVAL)
        {
            FetchHotScenes();
        }
        else if ((Time.realtimeSinceStartup - lastTimeRefreshed) >= SCENES_UPDATE_INTERVAL)
        {
            ProcessHotScenes();
        }
        else
        {
            loadingSpinner.SetActive(false);
        }
        scrollRect.verticalNormalizedPosition = 1;
    }

    void FetchHotScenes()
    {
        loadingSpinner.SetActive(true);

        WebInterface.FetchHotScenes();

        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        HotScenesController.i.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    void OnFetchHotScenes()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;

        mapDataController.ClearPending();
        ProcessHotScenes();
    }

    void ProcessHotScenes()
    {
        lastTimeRefreshed = Time.realtimeSinceStartup;

        List<Vector2Int> cellsToHide = new List<Vector2Int>(activeHotSceneViews.Keys);

        for (int i = 0; i < HotScenesController.i.hotScenesList.Count; i++)
        {
            cellsToHide.Remove(HotScenesController.i.hotScenesList[i].baseCoords);
            ProcessReceivedHotScene(HotScenesController.i.hotScenesList[i], i);
        }

        for (int i = 0; i < cellsToHide.Count; i++)
        {
            RemoveActiveHotSceneCell(cellsToHide[i]);
        }
    }

    void ProcessReceivedHotScene(HotScenesController.HotSceneInfo hotSceneInfo, int priority)
    {
        Vector2Int baseCoords = hotSceneInfo.baseCoords;
        HotSceneCellView hotSceneView = null;

        if (cachedHotScenes.ContainsKey(baseCoords))
        {
            hotSceneView = cachedHotScenes[baseCoords];
            if (!hotSceneView) return;
        }
        else
        {
            hotSceneView = hotScenesViewPool.GetView();
            cachedHotScenes.Add(baseCoords, hotSceneView);
        }

        hotSceneView.transform.SetSiblingIndex(priority);

        ICrowdDataView crowdView = hotSceneView;
        crowdView.SetCrowdInfo(hotSceneInfo);

        IMapDataView mapView = hotSceneView;

        mapDataController.SetMinimapData(baseCoords, mapView,
            (resolvedView) =>
            {
                if (!IsHotSceneCellActive(baseCoords))
                {
                    AddActiveHotSceneCell(baseCoords, hotSceneView);
                }
                loadingSpinner.SetActive(false);
            },
            (rejectedView) =>
            {
                hotScenesViewPool.PoolView(hotSceneView);
                cachedHotScenes[baseCoords] = null;
            });
    }

    void AddActiveHotSceneCell(Vector2Int coords, BaseSceneCellView view)
    {
        if (view == null) return;

        view.gameObject.SetActive(true);
        activeHotSceneViews.Add(coords, view);
        friendsController.AddHandler(view);
    }

    bool IsHotSceneCellActive(Vector2Int coords)
    {
        return activeHotSceneViews.ContainsKey(coords);
    }

    void RemoveActiveHotSceneCell(Vector2Int coords)
    {
        BaseSceneCellView view;
        if (activeHotSceneViews.TryGetValue(coords, out view))
        {
            view.gameObject.SetActive(false);
            friendsController.RemoveHandler(view);
        }

        activeHotSceneViews.Remove(coords);
    }

    void OnDestroy()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        hotScenesViewPool?.Dispose();
    }
}
