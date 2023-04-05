using System;
using System.Collections.Generic;
using System.Linq;
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

    public IReadOnlyCollection<MinimapSceneInfo> SceneInfos => scenesInfo;

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

                // NOTE: This removes outdated information for a particular parcel. Subsequent calls to update the
                // information for a parcel must override previously submitted information.
                sceneInfoMap.Remove(sceneInfo.parcels[i]);

            sceneInfoMap.Add(sceneInfo.parcels[i], sceneInfo);
        }

        scenesInfo.Add(sceneInfo);

        // it's not clear why we invoke the callback if `scenesInfo` already contains the scene
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
    public class MinimapSceneInfo : IEquatable<MinimapSceneInfo>
    {
        public string name;
        public TileType type;
        public List<Vector2Int> parcels;

        public bool isPOI;
        public string owner;
        public string description;
        public string previewImageUrl;

        [NonSerialized] private int? cachedHash;

        public bool Equals(MinimapSceneInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            // skip `previewImageUrl` on purpose
            return name == other.name
                   && type == other.type
                   && parcels.SequenceEqual(other.parcels)
                   && isPOI == other.isPOI
                   && owner == other.owner
                   && description == other.description;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MinimapSceneInfo)obj);
        }

        public override int GetHashCode() =>
            cachedHash ??= HashCode.Combine(name, (int)type, GetParcelsHashCode(), isPOI, owner, description);

        private int GetParcelsHashCode()
        {
            if (parcels == null || parcels.Count == 0)
                return 0;

            var hashCode = parcels[0].GetHashCode();

            for (var i = 1; i < parcels.Count; i++)
                hashCode = HashCode.Combine(hashCode, parcels[i].GetHashCode());

            return hashCode;
        }
    }
}
