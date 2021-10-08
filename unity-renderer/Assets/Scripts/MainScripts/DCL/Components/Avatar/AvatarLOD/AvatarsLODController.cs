using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        internal const float RENDERED_DOT_PRODUCT_ANGLE = 0.25f;
        private const float NAMES_OVERLAPPING_TOLERANCE = 0.5f;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private BaseVariable<float> simpleAvatarDistance => DataStore.i.avatarsLOD.simpleAvatarDistance;
        private BaseVariable<float> LODDistance => DataStore.i.avatarsLOD.LODDistance;
        private BaseVariable<int> maxAvatars => DataStore.i.avatarsLOD.maxAvatars;
        private BaseVariable<int> maxImpostors => DataStore.i.avatarsLOD.maxImpostors;
        private Vector3 cameraPosition;
        private Vector3 cameraForward;
        private GPUSkinningThrottlingCurveSO gpuSkinningThrottlingCurve;
        private RectsOverlappingTracker rectsOverlappingTracker = new RectsOverlappingTracker(NAMES_OVERLAPPING_TOLERANCE);

        internal readonly Dictionary<string, IAvatarLODController> lodControllers = new Dictionary<string, IAvatarLODController>();
        internal bool enabled;
        private UnityEngine.Camera mainCamera;

        public AvatarsLODController()
        {
            gpuSkinningThrottlingCurve = Resources.Load<GPUSkinningThrottlingCurveSO>("GPUSkinningThrottlingCurve");
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            KernelConfig.i.OnChange += OnKernelConfigChanged;
                            OnKernelConfigChanged(config, null);
                        });
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            if (enabled == current.features.enableAvatarLODs)
                return;
            Initialize(current);
        }

        internal void Initialize(KernelConfigModel config)
        {
            enabled = config.features.enableAvatarLODs;
            if (!enabled)
                return;

            foreach (IAvatarLODController lodController in lodControllers.Values)
            {
                lodController.Dispose();
            }
            lodControllers.Clear();

            foreach (var keyValuePair in otherPlayers.Get())
            {
                RegisterAvatar(keyValuePair.Key, keyValuePair.Value);
            }
            otherPlayers.OnAdded += RegisterAvatar;
            otherPlayers.OnRemoved += UnregisterAvatar;
        }

        public void RegisterAvatar(string id, Player player)
        {
            if (!enabled || lodControllers.ContainsKey(id))
                return;

            lodControllers.Add(id, CreateLodController(player));
        }

        protected internal virtual IAvatarLODController CreateLodController(Player player) => new AvatarLODController(player);

        public void UnregisterAvatar(string id, Player player)
        {
            if (!lodControllers.ContainsKey(id))
                return;

            lodControllers[id].Dispose();
            lodControllers.Remove(id);
        }

        public void Update()
        {
            if (!enabled)
                return;

            cameraPosition = CommonScriptableObjects.cameraPosition.Get();
            cameraForward = CommonScriptableObjects.cameraForward.Get();

            UpdateAllLODs(maxAvatars.Get(), maxImpostors.Get());
            UpdateLODsBillboard();
        }

        internal void UpdateLODsBillboard()
        {
            foreach (var kvp in lodControllers)
            {
                Player player = kvp.Value.player;

                if (!IsInFrontOfCamera(player.worldPosition))
                    continue;

                Vector3 previousForward = player.forwardDirection;
                Vector3 lookAtDir = (cameraPosition - player.worldPosition).normalized;

                lookAtDir.y = previousForward.y;
                player.renderer.SetImpostorForward(lookAtDir);
            }
        }

        internal void UpdateAllLODs(int maxAvatars = DataStore.DataStore_AvatarsLOD.DEFAULT_MAX_AVATAR, int maxImpostors = DataStore.DataStore_AvatarsLOD.DEFAULT_MAX_IMPOSTORS)
        {
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
            int avatarsCount = 0; //Full Avatar + Simple Avatar
            int impostorCount = 0; //Impostor

            float lodDistance = LODDistance.Get();
            float simpleAvatarDistance = this.simpleAvatarDistance.Get();
            Vector3 ownPlayerPosition = CommonScriptableObjects.playerUnityPosition.Get();

            rectsOverlappingTracker.Reset();

            (IAvatarLODController lodController, float distance)[] lodControllersByDistance = ComposeLODControllersSortedByDistance(lodControllers.Values, ownPlayerPosition);
            for (int index = 0; index < lodControllersByDistance.Length; index++)
            {
                (IAvatarLODController lodController, float distance) = lodControllersByDistance[index];
                if (distance < 0) //Behind camera
                {
                    lodController.SetNameVisible(false);
                    lodController.SetInvisible();
                    continue;
                }

                if (avatarsCount < maxAvatars)
                {
                    lodController.SetThrottling((int)gpuSkinningThrottlingCurve.curve.Evaluate(distance));
                    if (distance < simpleAvatarDistance)
                        lodController.SetFullAvatar();
                    else
                        lodController.SetSimpleAvatar();
                    avatarsCount++;

                    if (mainCamera == null)
                        lodController.SetNameVisible(true);
                    else
                        lodController.SetNameVisible(rectsOverlappingTracker.RegisterRect(lodController.player.playerName.ScreenSpaceRect(mainCamera)));
                    continue;
                }

                lodController.SetNameVisible(false);
                if (impostorCount < maxImpostors)
                {
                    lodController.SetImpostor();
                    lodController.UpdateImpostorTint(distance);
                    impostorCount++;
                    continue;
                }

                lodController.SetInvisible();
            }
        }

        private (IAvatarLODController lodController, float distance)[] ComposeLODControllersSortedByDistance(IEnumerable<IAvatarLODController> lodControllers, Vector3 ownPlayerPosition)
        {
            (IAvatarLODController lodController, float distance)[] lodControllersWithDistance = lodControllers.Select(x => (x, DistanceToOwnPlayer(x.player, ownPlayerPosition))).ToArray();
            Array.Sort(lodControllersWithDistance, (x, y) => x.distance.CompareTo(y.distance));
            return lodControllersWithDistance;
        }

        /// <summary>
        /// Returns -1 if player is not in front of camera or not found
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float DistanceToOwnPlayer(Player player, Vector3 ownPlayerPosition)
        {
            if (player == null || !IsInFrontOfCamera(player.worldPosition))
                return -1;

            return Vector3.Distance(ownPlayerPosition, player.worldPosition);
        }

        private bool IsInFrontOfCamera(Vector3 position) { return Vector3.Dot(cameraForward, (position - cameraPosition).normalized) >= RENDERED_DOT_PRODUCT_ANGLE; }

        public void Dispose()
        {
            KernelConfig.i.OnChange -= OnKernelConfigChanged;
            foreach (IAvatarLODController lodController in lodControllers.Values)
            {
                lodController.Dispose();
            }

            otherPlayers.OnAdded -= RegisterAvatar;
            otherPlayers.OnRemoved -= UnregisterAvatar;
        }
    }
}