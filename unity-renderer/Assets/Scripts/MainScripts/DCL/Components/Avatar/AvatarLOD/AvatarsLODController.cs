using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        internal const float SIMPLE_AVATAR_DISTANCE = 10f;
        private const int DEFAULT_MAX_AVATAR_VISIBLE = 70;
        private const int DEFAUL_MAX_PLAYER_VISIBLE = 200;

        internal readonly Dictionary<string, IAvatarLODController> lodControllers = new Dictionary<string, IAvatarLODController>();
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
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

            UpdateAllLODs();
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

        internal void UpdateAllLODs(int maxAvatarVisible = DEFAULT_MAX_AVATAR_VISIBLE, int maxPlayerVisible = DEFAUL_MAX_PLAYER_VISIBLE)
        {
            int avatarsCount = 0; //Full Avatar
            int impostorCount = 0; //Impostor

            //Sort all the players by distance to maximize the amount of HQ avatars.
            List<(string playerid, IAvatarLODController lodController, float distance)> sortedPlayers = lodControllers
                                                                                                        .Select(x => (playerid: x.Key, lodController: x.Value, distance: DistanceToOwnPlayer(x.Key)))
                                                                                                        .OrderBy(x => x.distance)
                                                                                                        .ToList();
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                if (sortedPlayers[i].distance < 0) //Behind camera
                {
                    sortedPlayers[i].lodController.SetInvisible();
                    continue;
                }

                if (sortedPlayers[i].distance < DataStore.i.avatarsLOD.LODDistance.Get())
                {
                    if (avatarsCount >= maxAvatarVisible)
                    {
                        sortedPlayers[i].lodController.SetInvisible();
                        continue;
                    }
                    if (sortedPlayers[i].distance < SIMPLE_AVATAR_DISTANCE)
                        sortedPlayers[i].lodController.SetAvatarState();
                    else
                        sortedPlayers[i].lodController.SetSimpleAvatar();

                    avatarsCount++;
                    continue;
                }

                if (avatarsCount + impostorCount < maxPlayerVisible)
                {
                    sortedPlayers[i].lodController.SetImpostorState();
                    impostorCount++;
                    continue;
                }

                sortedPlayers[i].lodController.SetInvisible();
            }
        }

        /// <summary>
        /// Returns -1 if player is not in front of camera or not found
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float DistanceToOwnPlayer(string playerId)
        {
            if (!otherPlayers.TryGetValue(playerId, out Player player))
                return -1;

            if (!IsInFrontOfCamera(player))
                return -1;
            return Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), player.worldPosition);
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