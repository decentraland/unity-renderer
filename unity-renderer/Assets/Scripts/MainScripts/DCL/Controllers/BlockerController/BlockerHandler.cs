using System;
using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using DCL.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Controllers
{
    /// <summary>
    /// This class is the loading blockers composite and instancing handler.
    /// <br/>
    /// Responsibilities include:<br/>
    /// - Handling hide/show of blockers through its IBlockerAnimationHandler<br/>
    /// - Keeping track of all blockers<br/>
    /// </summary>
    public class BlockerInstanceHandler : IBlockerInstanceHandler
    {
        static GameObject blockerPrefab;
        private bool blockerPrefabDirty;

        const string PARCEL_BLOCKER_POOL_NAME = "ParcelBlocker";

        Vector3 auxPosVec = new Vector3();
        Vector3 auxScaleVec = new Vector3();

        Dictionary<Vector2Int, IPoolableObject> blockers = new Dictionary<Vector2Int, IPoolableObject>();

        private IBlockerAnimationHandler animationHandler;
        private ICullingController cullingController;
        private Transform parent;

        public void Initialize(IBlockerAnimationHandler animationHandler, ICullingController cullingController)
        {
            this.cullingController = cullingController;
            this.animationHandler = animationHandler;
        }

        public BlockerInstanceHandler()
        {
            RenderProfileManifest.i.OnChangeProfile += OnChangeProfile;
            OnChangeProfile(RenderProfileManifest.i.currentProfile);
        }

        private void OnChangeProfile(RenderProfileWorld profile)
        {
            if (profile == null)
                return;

            blockerPrefabDirty = true;
            blockerPrefab = profile.loadingBlockerPrefab;
        }

        public void ShowBlocker(Vector2Int pos, bool instant = false)
        {
            if (blockerPrefabDirty)
            {
                blockerPrefabDirty = false;
                EnsureBlockerPool();
            }

            float centerOffset = ParcelSettings.PARCEL_SIZE / 2;
            PoolableObject blockerPoolable = PoolManager.i.Get(PARCEL_BLOCKER_POOL_NAME);
            GameObject blockerGo = blockerPoolable.gameObject;
            BoxCollider blockerCollider = blockerGo.GetComponent<BoxCollider>();

            Vector3 blockerPos = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(pos.x, pos.y));

            auxPosVec.x = blockerPos.x + centerOffset;
            auxPosVec.z = blockerPos.z + centerOffset;
            auxPosVec.y = 8;

            Transform blockerTransform = blockerGo.transform;
            blockerTransform.SetParent(parent, false);
            blockerTransform.position = auxPosVec;
            blockerTransform.localScale = Vector3.one * 16;

            blockerCollider.size = Vector3.one + (Vector3.up * auxScaleVec.y);
            blockerCollider.center = Vector3.up * ((auxScaleVec.y / 2) - 0.5f);

#if UNITY_EDITOR
            blockerGo.name = "BLOCKER " + pos;
#endif

            blockers.Add(pos, blockerPoolable);

            if (!instant)
                animationHandler.FadeIn(blockerGo);

            cullingController?.MarkDirty();
        }

        private void EnsureBlockerPool()
        {
            // We need to manually create the Pool for empty game objects if it doesn't exist
            if (PoolManager.i.ContainsPool(PARCEL_BLOCKER_POOL_NAME))
            {
                PoolManager.i.RemovePool(PARCEL_BLOCKER_POOL_NAME);
            }

            GameObject go = Object.Instantiate(blockerPrefab);
            Pool pool = PoolManager.i.AddPool(PARCEL_BLOCKER_POOL_NAME, go);
            pool.persistent = true;
            pool.ForcePrewarm();
        }

        public void SetParent(Transform parent) { this.parent = parent; }

        public void HideBlocker(Vector2Int coords, bool instant = false)
        {
            if (instant)
            {
                ReleaseBlocker(coords);
                return;
            }

            animationHandler.FadeOut(
                blockers[coords].gameObject,
                () => ReleaseBlocker(coords)
            );
        }

        private void ReleaseBlocker(Vector2Int coords)
        {
            if (!blockers.ContainsKey(coords))
                return;

            blockers[coords].Release();
            blockers.Remove(coords);
        }

        public Dictionary<Vector2Int, IPoolableObject> GetBlockers() { return new Dictionary<Vector2Int, IPoolableObject>(blockers); }

        public void DestroyAllBlockers()
        {
            var keys = blockers.Keys.ToArray();

            for (var i = 0; i < keys.Length; i++)
            {
                ReleaseBlocker(keys[i]);
            }

            blockers.Clear();
        }
    }
}