using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Controllers
{
    public class BlockerHandler
    {
        private List<PoolableObject> blockers = new List<PoolableObject>();

        private static GameObject blockerPrefab;
        const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";
        private const string PARCEL_BLOCKER_PREFAB = "Prefabs/ParcelBlocker";

        Vector3 auxPosVec = new Vector3();
        Vector3 auxScaleVec = new Vector3();

        private static Vector2Int[] aroundOffsets =
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

        public BlockerHandler()
        {
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


        public void SetupBlockers(HashSet<Vector2Int> parcels, float height, Transform parent)
        {
            CleanBlockers();

            auxScaleVec.x = ParcelSettings.PARCEL_SIZE;
            auxScaleVec.y = height;
            auxScaleVec.z = ParcelSettings.PARCEL_SIZE;

            auxPosVec.y = (height - 1) / 2;

            float centerOffset = ParcelSettings.PARCEL_SIZE / 2;

            using (var it = parcels.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    Vector2Int pos = it.Current;

                    bool isSurrounded = true;

                    for (int i1 = 0; i1 < aroundOffsets.Length; i1++)
                    {
                        Vector2Int o = aroundOffsets[i1];

                        if (!parcels.Contains(new Vector2Int(pos.x + o.x, pos.y + o.y)))
                        {
                            isSurrounded = false;
                            break;
                        }
                    }

                    if (isSurrounded)
                        continue;

                    PoolableObject blockerPoolable = PoolManager.i.Get(PARCEL_BLOCKER_POOL_NAME);
                    Transform blockerTransform = blockerPoolable.gameObject.transform;

                    blockerTransform.SetParent(parent, false);

                    if (DCLCharacterController.i != null)
                        blockerTransform.position = DCLCharacterController.i.characterPosition.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y));
                    else
                        blockerTransform.position = Utils.GridToWorldPosition(pos.x, pos.y);

                    auxPosVec.x = blockerTransform.position.x + centerOffset;
                    auxPosVec.z = blockerTransform.position.z + centerOffset;

                    blockerTransform.position = auxPosVec;
                    blockerTransform.localScale = auxScaleVec;

                    blockers.Add(blockerPoolable);
                }
            }
        }

        public void CleanBlockers()
        {
            for (int i = 0; i < blockers.Count; i++)
            {
                blockers[i].Release();
            }

            blockers = new List<PoolableObject>();
        }
    }
}