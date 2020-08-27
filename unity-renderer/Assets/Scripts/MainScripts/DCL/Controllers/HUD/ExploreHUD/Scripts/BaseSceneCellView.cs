using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using DCL.Helpers;

internal class BaseSceneCellView : BaseCellView, IMapDataView, IFriendTrackerHandler
{
    public delegate void JumpInDelegate(Vector2Int coords, string serverName, string layerName);

    static public event JumpInDelegate OnJumpIn;

    public static event Action<BaseSceneCellView> OnInfoButtonPointerDown;
    public static event Action OnInfoButtonPointerExit;

    [SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] Button_OnPointerDown jumpIn;
    [SerializeField] ExploreFriendsView friendsView;
    [SerializeField] protected UIHoverCallback sceneInfoButton;

    MinimapMetadata.MinimapSceneInfo mapInfo;
    Vector2Int baseCoords;

    ViewPool<ExploreFriendsView> friendPool;
    Dictionary<string, ExploreFriendsView> friendViewById = new Dictionary<string, ExploreFriendsView>();

    protected virtual void Awake()
    {
        friendPool = new ViewPool<ExploreFriendsView>(friendsView, 0);

        // NOTE: we don't use the pointer down callback to avoid being mistakenly pressed while dragging
        jumpIn.onClick.AddListener(JumpInPressed);

        sceneInfoButton.OnPointerDown += () => OnInfoButtonPointerDown?.Invoke(this);
        sceneInfoButton.OnPointerExit += () => OnInfoButtonPointerExit?.Invoke();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        friendPool.Dispose();
    }

    public virtual void JumpInPressed()
    {
        if (mapInfo != null)
        {
            JumpIn(baseCoords, null, null);
        }
    }

    protected void JumpIn(Vector2Int coords, string serverName, string layerName)
    {
        OnJumpIn?.Invoke(coords, serverName, layerName);
    }

    MinimapMetadata.MinimapSceneInfo IMapDataView.GetMinimapSceneInfo()
    {
        return mapInfo;
    }

    bool IMapDataView.HasMinimapSceneInfo()
    {
        return mapInfo != null;
    }

    void IMapDataView.SetBaseCoord(Vector2Int coords)
    {
        baseCoords = coords;
    }

    Vector2Int IMapDataView.GetBaseCoord()
    {
        return baseCoords;
    }

    void IMapDataView.SetMinimapSceneInfo(MinimapMetadata.MinimapSceneInfo info)
    {
        mapInfo = info;
        sceneName.text = info.name;

        if (GetThumbnail() == null)
        {
            string url = mapInfo.previewImageUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = MapUtils.GetMarketPlaceThumbnailUrl(mapInfo, 196, 143, 50);
            }

            FetchThumbnail(url);
        }
    }

    bool IMapDataView.ContainCoords(Vector2Int coords)
    {
        if (mapInfo == null) return false;
        return mapInfo.parcels.Contains(coords);
    }

    void IFriendTrackerHandler.OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        var view = friendPool.GetView();
        view.SetUserProfile(profile, backgroundColor);
        friendViewById.Add(profile.userId, view);
    }

    void IFriendTrackerHandler.OnFriendRemoved(UserProfile profile)
    {
        ExploreFriendsView view;
        if (friendViewById.TryGetValue(profile.userId, out view))
        {
            friendPool.PoolView(view);
            friendViewById.Remove(profile.userId);
        }
    }

    bool IFriendTrackerHandler.ContainCoords(Vector2Int coords)
    {
        return ((IMapDataView) this).ContainCoords(coords);
    }
}