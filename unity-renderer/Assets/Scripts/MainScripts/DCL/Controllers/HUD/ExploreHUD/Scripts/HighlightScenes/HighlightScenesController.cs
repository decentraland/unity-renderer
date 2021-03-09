using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;

internal class HighlightScenesController : MonoBehaviour
{
    const float SCENES_UPDATE_INTERVAL = 60;
    private const float SCENE_FIRST_LOAD_DELAY = 0.1f;

    [SerializeField] HotSceneCellView hotsceneBaseCellView;
    [SerializeField] ScrollRect scrollRect;

    Dictionary<Vector2Int, HotSceneCellView> cachedHotScenes = new Dictionary<Vector2Int, HotSceneCellView>();
    Dictionary<Vector2Int, HotSceneCellView> activeHotSceneViews = new Dictionary<Vector2Int, HotSceneCellView>();

    FriendTrackerController friendsController;

    ViewPool<HotSceneCellView> hotScenesViewPool;

    float lastTimeRefreshed = 0;
    private Coroutine firstLoadAnimRoutine = null;

    public void Initialize(FriendTrackerController friendsController)
    {
        this.friendsController = friendsController;
        hotScenesViewPool = new ViewPool<HotSceneCellView>(hotsceneBaseCellView, 9);
    }

    public void RefreshIfNeeded()
    {
        bool isFirstTimeLoad = cachedHotScenes.Count == 0;
        if (isFirstTimeLoad && !ExploreHUDController.isTest)
        {
            firstLoadAnimRoutine = StartCoroutine(FirstTimeLoadingRoutine());
        }
        else if (HotScenesController.i.timeSinceLastUpdate >= SCENES_UPDATE_INTERVAL)
        {
            FetchHotScenes();
        }
        else if ((Time.realtimeSinceStartup - lastTimeRefreshed) >= SCENES_UPDATE_INTERVAL)
        {
            ProcessHotScenes();
        }

        scrollRect.verticalNormalizedPosition = 1;
    }

    void FetchHotScenes()
    {
        WebInterface.FetchHotScenes();

        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        HotScenesController.i.OnHotSceneListFinishUpdating += OnFetchHotScenes;
    }

    void OnFetchHotScenes()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
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
            hotSceneView.Initialize();
            cachedHotScenes.Add(baseCoords, hotSceneView);
        }

        hotSceneView.transform.SetSiblingIndex(priority);

        if (!hotSceneView.gameObject.activeSelf)
        {
            hotSceneView.gameObject.SetActive(true);
        }

        if (!IsHotSceneCellActive(baseCoords))
        {
            AddActiveHotSceneCell(baseCoords, hotSceneView);
        }

        hotSceneView.Setup(hotSceneInfo);
        friendsController.AddHandler(hotSceneView.friendsHandler);
    }

    void AddActiveHotSceneCell(Vector2Int coords, HotSceneCellView view)
    {
        activeHotSceneViews.Add(coords, view);
    }

    bool IsHotSceneCellActive(Vector2Int coords)
    {
        return activeHotSceneViews.ContainsKey(coords);
    }

    void RemoveActiveHotSceneCell(Vector2Int coords)
    {
        if (activeHotSceneViews.TryGetValue(coords, out HotSceneCellView view))
        {
            view.gameObject.SetActive(false);
            view.Clear();
            friendsController.RemoveHandler(view.friendsHandler);
        }

        activeHotSceneViews.Remove(coords);
    }

    private void OnDisable()
    {
        if (!(firstLoadAnimRoutine is null))
        {
            StopCoroutine(firstLoadAnimRoutine);
            firstLoadAnimRoutine = null;
        }
    }

    void OnDestroy()
    {
        HotScenesController.i.OnHotSceneListFinishUpdating -= OnFetchHotScenes;
        hotScenesViewPool?.Dispose();
    }

    IEnumerator FirstTimeLoadingRoutine()
    {
        using (var iterator = hotScenesViewPool.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                if (iterator.Current is null) continue;

                iterator.Current.gameObject.SetActive(true);
                yield return new WaitForSeconds(SCENE_FIRST_LOAD_DELAY);
            }
        }
        FetchHotScenes();
    }
}
