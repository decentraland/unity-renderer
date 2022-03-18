using System;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Controllers
{
    /// <summary>
    /// This class is the domain-specific glue for BlockerInstanceHandler.
    /// <br/><br/>
    /// Responsibilities:<br/>
    /// - Spawning blockers depending on scene state<br/>
    /// - Moving blockers when the world is repositioned<br/>
    /// - Handling lifecycle of BlockerInstanceHandler<br/>
    /// </summary>
    public class WorldBlockersController : IWorldBlockersController
    {
        public bool enabled = true;

        Transform blockersParent;

        ISceneHandler sceneHandler;
        IBlockerInstanceHandler blockerInstanceHandler;

        HashSet<Vector2Int> blockersToRemove = new HashSet<Vector2Int>();
        HashSet<Vector2Int> blockersToAdd = new HashSet<Vector2Int>();

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

        void OnRendererStateChange(bool newValue, bool oldValue)
        {
            blockerInstanceHandler.SetCollision(newValue);
            
            if (newValue && DataStore.i.debugConfig.isDebugMode.Get())
                SetEnabled(false);
        }

        public WorldBlockersController(IBlockerInstanceHandler blockerInstanceHandler = null, ISceneHandler sceneHandler = null)
        {
            this.sceneHandler = sceneHandler;
            this.blockerInstanceHandler = blockerInstanceHandler;
        }

        public void SetupWorldBlockers()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            
            if (!enabled || sceneHandler == null)
                return;

            SetupWorldBlockers(sceneHandler.GetAllLoadedScenesCoords());
        }

        public void SetEnabled(bool targetValue)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            
            enabled = targetValue;

            if (!enabled)
                blockerInstanceHandler.DestroyAllBlockers();
        }

        void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            var newPosition = PositionUtils.WorldToUnityPosition(Vector3.zero); // Blockers parent original position
            blockersParent.position = newPosition;
        }

        public void Dispose()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            
            CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
            blockerInstanceHandler.DestroyAllBlockers();

            if (blockersParent != null)
                Object.Destroy(blockersParent.gameObject);

            enabled = false;
        }

        public void Initialize()
        {
            enabled = true;
            blockersParent = new GameObject("WorldBlockers").transform;
            blockersParent.position = Vector3.zero;

            if ( blockerInstanceHandler == null )
            {
                var blockerAnimationHandler = new BlockerAnimationHandler();
                blockerInstanceHandler = new BlockerInstanceHandler(blockerAnimationHandler);
            }

            if ( this.sceneHandler == null )
                this.sceneHandler = DCL.Environment.i.world.state;

            blockerInstanceHandler.SetParent(blockersParent);

            CommonScriptableObjects.worldOffset.OnChange -= OnWorldReposition;
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;

            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChange;
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        }

        internal void SetupWorldBlockers(HashSet<Vector2Int> allLoadedParcelCoords)
        {
            if (allLoadedParcelCoords.Count == 0)
                return;

            blockersToRemove.Clear();
            blockersToAdd.Clear();

            var blockers = blockerInstanceHandler.GetBlockers();

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
                blockerInstanceHandler.HideBlocker(coords, false);
            }

            // Add missing blockers
            foreach (var coords in blockersToAdd)
            {
                blockerInstanceHandler.ShowBlocker(coords, false, CommonScriptableObjects.rendererState.Get());
            }
        }
    }
}