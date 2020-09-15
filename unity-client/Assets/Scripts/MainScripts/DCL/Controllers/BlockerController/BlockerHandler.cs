using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public interface IBlockerHandler
    {
        void SetupGlobalBlockers(HashSet<Vector2Int> allLoadedParcelCoords, float height, Transform parent);
        void CleanBlockers();
        Dictionary<Vector2Int, PoolableObject> GetBlockers();
    }

    public class BlockerHandler : IBlockerHandler
    {
        static GameObject blockerPrefab;

        const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";
        const string PARCEL_BLOCKER_PREFAB = "Prefabs/ParcelBlocker";

        Vector3 auxPosVec = new Vector3();
        Vector3 auxScaleVec = new Vector3();
        Dictionary<Vector2Int, PoolableObject> blockers = new Dictionary<Vector2Int, PoolableObject>();
        HashSet<Vector2Int> blockersToRemove = new HashSet<Vector2Int>();
        HashSet<Vector2Int> blockersToAdd = new HashSet<Vector2Int>();
        DCLCharacterPosition characterPosition;

        static Vector2Int[] aroundOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1)
        };

        public BlockerHandler(DCLCharacterPosition characterPosition)
        {
            this.characterPosition = characterPosition;

            if (blockerPrefab == null)
                blockerPrefab = Resources.Load<GameObject>(PARCEL_BLOCKER_PREFAB);

            // We need to manually create the Pool for empty game objects if it doesn't exist
            if (!PoolManager.i.ContainsPool(PARCEL_BLOCKER_POOL_NAME))
            {
                GameObject go = Object.Instantiate(blockerPrefab);
                Pool pool = PoolManager.i.AddPool(PARCEL_BLOCKER_POOL_NAME, go);
                pool.persistent = true;
                pool.ForcePrewarm();
            }
        }

        protected void InstantiateBlocker(Vector2Int pos, Transform parent)
        {
            float centerOffset = ParcelSettings.PARCEL_SIZE / 2;
            PoolableObject blockerPoolable = PoolManager.i.Get(PARCEL_BLOCKER_POOL_NAME);
            Transform blockerTransform = blockerPoolable.gameObject.transform;

            blockerTransform.SetParent(parent, false);
            blockerTransform.position = this.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y));

            auxPosVec.x = blockerTransform.position.x + centerOffset;
            auxPosVec.z = blockerTransform.position.z + centerOffset;

            blockerTransform.position = auxPosVec;
            blockerTransform.localScale = auxScaleVec;

#if UNITY_EDITOR
            blockerPoolable.gameObject.name = "BLOCKER " + pos;
#endif

            blockers.Add(pos, blockerPoolable);
        }

        public void SetupGlobalBlockers(HashSet<Vector2Int> allLoadedParcelCoords, float height, Transform parent)
        {
            if (allLoadedParcelCoords.Count == 0) return;

            blockersToRemove.Clear();
            blockersToAdd.Clear();

            auxScaleVec.x = ParcelSettings.PARCEL_SIZE;
            auxScaleVec.y = height;
            auxScaleVec.z = ParcelSettings.PARCEL_SIZE;

            auxPosVec.y = (height - 1) / 2;

            // Detect blockers to be removed
            foreach (var item in blockers)
            {
                if (allLoadedParcelCoords.Contains(item.Key))
                {
                    blockersToRemove.Add(item.Key);
                }
                else
                {
                    bool foundAroundLoadedScenes = false;
                    for (int i = 0; i < aroundOffsets.Length; i++)
                    {
                        Vector2Int offset = aroundOffsets[i];
                        Vector2Int checkedPosition = new Vector2Int(item.Key.x + offset.x, item.Key.y + offset.y);

                        if (allLoadedParcelCoords.Contains(checkedPosition))
                        {
                            foundAroundLoadedScenes = true;
                            break;
                        }
                    }

                    if (!foundAroundLoadedScenes)
                        blockersToRemove.Add(item.Key);
                }
            }

            // Detect missing blockers to be added
            using (var it = allLoadedParcelCoords.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    Vector2Int pos = it.Current;

                    for (int i = 0; i < aroundOffsets.Length; i++)
                    {
                        Vector2Int offset = aroundOffsets[i];
                        Vector2Int checkedPosition = new Vector2Int(pos.x + offset.x, pos.y + offset.y);

                        if (!allLoadedParcelCoords.Contains(checkedPosition) && !blockers.ContainsKey(checkedPosition))
                        {
                            blockersToAdd.Add(checkedPosition);
                        }
                    }
                }
            }

            // Remove extra blockers
            foreach (var coords in blockersToRemove)
            {
                blockers[coords].Release();
                blockers.Remove(coords);
            }

            // Add missing blockers
            foreach (var coords in blockersToAdd)
            {
                InstantiateBlocker(coords, parent);
            }
        }

        public Dictionary<Vector2Int, PoolableObject> GetBlockers()
        {
            return new Dictionary<Vector2Int, PoolableObject>(blockers);
        }

        public void CleanBlockers()
        {
            foreach (var blocker in blockers)
            {
                blocker.Value.Release();
            }

            blockers.Clear();
        }
    }
}