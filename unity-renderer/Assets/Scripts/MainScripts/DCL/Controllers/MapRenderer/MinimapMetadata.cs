using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

[CreateAssetMenu(fileName = "MinimapMetadata", menuName = "MinimapMetadata")]
public class MinimapMetadata : ScriptableObject
{
    public enum TileType
    {
        MyParcel = 0,
        MyParcelsOnSale = 1,
        MyEstates = 2,
        MyEstatesOnSale = 3,
        WithAccess = 4,
        District = 5,
        Contribution = 6,
        Roads = 7,
        Plaza = 8,
        Taken = 9,
        OnSale = 10,
        Unowned = 11,
        Background = 12,
        Loading = 13,
    }

    private static MinimapMetadata minimapMetadata;

    private readonly HashSet<MinimapSceneInfo> scenesInfo = new ();
    private readonly Dictionary<Vector2Int, MinimapSceneInfo> sceneInfoMap = new ();

    public event Action<MinimapSceneInfo> OnSceneInfoUpdated;

    public MinimapSceneInfo GetSceneInfo(int x, int y) =>
        sceneInfoMap.TryGetValue(new Vector2Int(x, y), out MinimapSceneInfo result)
            ? result
            : null;

    public void AddSceneInfo(MinimapSceneInfo sceneInfo)
    {
        if (scenesInfo.Contains(sceneInfo))
            return;

        int parcelsCount = sceneInfo.parcels.Count;

        for (var i = 0; i < parcelsCount; i++)
        {
            if (sceneInfoMap.ContainsKey(sceneInfo.parcels[i]))

                //NOTE(Brian): I intended at first to just return; here. But turns out kernel is sending
                //             overlapping coordinates, sending first gigantic 'Estate' and 'Roads' scenes to
                //             send the proper scenes later. This will be fixed when we implement the content v3 data
                //             plumbing.
                sceneInfoMap.Remove(sceneInfo.parcels[i]);

            sceneInfoMap.Add(sceneInfo.parcels[i], sceneInfo);
        }

        scenesInfo.Add(sceneInfo);

        OnSceneInfoUpdated?.Invoke(sceneInfo);
    }

    public void Clear()
    {
        scenesInfo.Clear();
        sceneInfoMap.Clear();
    }

    public static MinimapMetadata GetMetadata()
    {
        if (minimapMetadata == null)
            minimapMetadata = Resources.Load<MinimapMetadata>("ScriptableObjects/MinimapMetadata");

        return minimapMetadata;
    }

    [Serializable]
    public class MinimapSceneInfo
    {
        static readonly ProfilerMarker s_PreparePerfMarker = new ("Vitaly.Equals");

        public string name;
        public TileType type;
        public List<Vector2Int> parcels;

        public bool isPOI;
        public string owner;
        public string description;
        public string previewImageUrl;

        [NonSerialized]
        private int hashCode = -1;

        public override bool Equals(object obj)
        {
            using (s_PreparePerfMarker.Auto())
                return obj is MinimapSceneInfo info && Equals(info);
        }

        private bool Equals(MinimapSceneInfo other) =>
            this.GetHashCode() == other.GetHashCode();

        public override int GetHashCode()
        {
            if(hashCode == -1)
                hashCode = (name + type + parcels + isPOI + owner + description + previewImageUrl).GetHashCode();

            return hashCode;
        }

    }
}
