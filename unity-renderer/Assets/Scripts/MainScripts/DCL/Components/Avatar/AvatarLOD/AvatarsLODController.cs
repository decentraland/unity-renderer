using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        internal const float RENDERED_DOT_PRODUCT_ANGLE = 0.25f;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private BaseVariable<float> simpleAvatarDistance => DataStore.i.avatarsLOD.simpleAvatarDistance;
        private BaseVariable<float> LODDistance => DataStore.i.avatarsLOD.LODDistance;
        private BaseVariable<int> maxAvatars => DataStore.i.avatarsLOD.maxAvatars;
        private BaseVariable<int> maxImpostors => DataStore.i.avatarsLOD.maxImpostors;
        private Vector3 cameraPosition;
        private Vector3 cameraForward;

        internal readonly Dictionary<string, IAvatarLODController> lodControllers = new Dictionary<string, IAvatarLODController>();
        internal bool enabled;

        public AvatarsLODController()
        {
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(Initialize);
        }

        internal void Initialize(KernelConfigModel config)
        {
            enabled = config.features.enableAvatarLODs;
            if (!enabled)
                return;

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
            int avatarsCount = 0; //Full Avatar + Simple Avatar
            int impostorCount = 0; //Impostor

            //Cache .Get to boost performance. Also use squared values to boost distance comparison
            float lodDistance = LODDistance.Get() * LODDistance.Get();
            float squaredSimpleAvatarDistance = simpleAvatarDistance.Get() * simpleAvatarDistance.Get();
            Vector3 ownPlayerPosition = CommonScriptableObjects.playerUnityPosition.Get();

            (IAvatarLODController lodController, float sqrDistance)[] lodControllersByDistance = ComposeLODControllersSortedByDistance(lodControllers.Values, ownPlayerPosition);
            for (int index = 0; index < lodControllersByDistance.Length; index++)
            {
                (IAvatarLODController lodController, float sqrtDistance) = lodControllersByDistance[index];
                if (sqrtDistance < 0) //Behind camera
                {
                    lodController.SetInvisible();
                    continue;
                }

                //Nearby player
                if (sqrtDistance < lodDistance)
                {
                    if (avatarsCount < maxAvatars)
                    {
                        if (sqrtDistance < squaredSimpleAvatarDistance)
                            lodController.SetFullAvatar();
                        else
                            lodController.SetSimpleAvatar();
                        avatarsCount++;
                        continue;
                    }

                    lodController.SetInvisible();
                    continue;
                }

                if (avatarsCount < maxAvatars)
                {
                    lodController.SetSimpleAvatar();
                    avatarsCount++;
                    continue;
                }
                if (impostorCount < maxImpostors)
                {
                    lodController.SetImpostor();
                    impostorCount++;
                    continue;
                }

                lodController.SetInvisible();
            }
        }

        private (IAvatarLODController lodController, float sqrDistance)[] ComposeLODControllersSortedByDistance(IEnumerable<IAvatarLODController> lodControllers, Vector3 ownPlayerPosition)
        {
            (IAvatarLODController lodController, float sqrDistance)[] lodControllersWithDistance = lodControllers.Select(x => (x, SqrDistanceToOwnPlayer(x.player, ownPlayerPosition))).ToArray();
            Array.Sort(lodControllersWithDistance, (x, y) => x.sqrDistance.CompareTo(y.sqrDistance));
            return lodControllersWithDistance;
        }

        /// <summary>
        /// Returns -1 if player is not in front of camera or not found
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float SqrDistanceToOwnPlayer(Player player, Vector3 ownPlayerPosition)
        {
            if (player == null || !IsInFrontOfCamera(player.worldPosition))
                return -1;

            return Vector3.SqrMagnitude(ownPlayerPosition - player.worldPosition);
        }

        private bool IsInFrontOfCamera(Vector3 position) { return Vector3.Dot(cameraForward, (position - cameraPosition).normalized) >= RENDERED_DOT_PRODUCT_ANGLE; }

        public void Dispose()
        {
            foreach (IAvatarLODController lodController in lodControllers.Values)
            {
                lodController.Dispose();
            }

            otherPlayers.OnAdded -= RegisterAvatar;
            otherPlayers.OnRemoved -= UnregisterAvatar;
        }
    }
}