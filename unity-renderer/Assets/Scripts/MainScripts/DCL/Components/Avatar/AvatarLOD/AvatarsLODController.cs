using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private BaseVariable<float> simpleAvatarDistance => DataStore.i.avatarsLOD.simpleAvatarDistance;
        private BaseVariable<float> LODDistance => DataStore.i.avatarsLOD.LODDistance;
        private BaseVariable<int> maxAvatars => DataStore.i.avatarsLOD.maxAvatars;
        private BaseVariable<int> maxImpostors => DataStore.i.avatarsLOD.maxImpostors;

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

            UpdateAllLODs(maxAvatars.Get(), maxImpostors.Get());
            UpdateLODsBillboard();
        }

        internal void UpdateLODsBillboard()
        {
            foreach (var kvp in lodControllers)
            {
                otherPlayers.TryGetValue(kvp.Key, out Player player);
                Vector3 previousForward = player.forwardDirection;
                Vector3 lookAtDir = (player.worldPosition - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;
                player.renderer.SetImpostorForward(lookAtDir);
            }
        }

        internal void UpdateAllLODs(int maxAvatars = DataStore.DataStore_AvatarsLOD.DEFAULT_MAX_AVATAR, int maxImpostors = DataStore.DataStore_AvatarsLOD.DEFAULT_MAX_IMPOSTORS)
        {
            int avatarsCount = 0; //Full Avatar + Simple Avatar
            int impostorCount = 0; //Impostor

            //Cache .Get to boost performance. Also use squared data to boost distance comparison
            float lodDistance = LODDistance.Get() * LODDistance.Get();
            float squaredSimpleAvatarDistance = simpleAvatarDistance.Get() * simpleAvatarDistance.Get();
            Vector3 ownPlayerPosition = CommonScriptableObjects.playerUnityPosition.Get();

            (IAvatarLODController lodController, float sqrtDistance)[] data = lodControllers.Values.Select(x => (lodController: x, sqrtDistance: SqrtDistanceToOwnPlayer(x.player, ownPlayerPosition))).ToArray();
            Array.Sort(data, (x, y) => x.sqrtDistance.CompareTo(y.sqrtDistance));
            foreach (var player in data)
            {
                if (player.sqrtDistance < 0) //Behind camera
                {
                    continue;
                }

                //Nearby player
                if (player.sqrtDistance < lodDistance)
                {
                    if (avatarsCount < maxAvatars)
                    {
                        if (player.sqrtDistance < squaredSimpleAvatarDistance)
                            player.lodController.SetFullAvatar();
                        else
                            player.lodController.SetSimpleAvatar();
                        avatarsCount++;
                        continue;
                    }
                    player.lodController.SetInvisible();
                    continue;
                }

                if (avatarsCount < maxAvatars)
                {
                    player.lodController.SetSimpleAvatar();
                    avatarsCount++;
                    continue;
                }
                if (impostorCount < maxImpostors)
                {
                    player.lodController.SetImpostor();
                    impostorCount++;
                    continue;
                }

                player.lodController.SetInvisible();
            }
        }

        /// <summary>
        /// Returns -1 if player is not in front of camera or not found
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float SqrtDistanceToOwnPlayer(Player player, Vector3 ownPlayerPosition)
        {
            if (player == null || !IsInFrontOfCamera(player))
                return -1;
            return Mathf.Abs(Vector3.SqrMagnitude(ownPlayerPosition - player.worldPosition));
        }

        private bool IsInFrontOfCamera(Player player) => true;

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