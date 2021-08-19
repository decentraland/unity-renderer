using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        internal const float SIMPLE_AVATAR_DISTANCE = 10f;

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

        internal void UpdateAllLODs()
        {
            SortedList<float, IAvatarLODController> closeDistanceAvatars = new SortedList<float, IAvatarLODController>();
            foreach (var avatarKVP in lodControllers)
            {
                var lodController = avatarKVP.Value;
                var position = otherPlayers[avatarKVP.Key].worldPosition;
                float distanceToPlayer = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), position);
                bool isInLODDistance = distanceToPlayer >= DataStore.i.avatarsLOD.LODDistance.Get();

                if (isInLODDistance)
                {
                    lodController.SetImpostorState();
                }
                else
                {
                    while (closeDistanceAvatars.ContainsKey(distanceToPlayer))
                    {
                        distanceToPlayer += 0.0001f;
                    }
                    closeDistanceAvatars.Add(distanceToPlayer, lodController);
                }
            }

            int closeDistanceAvatarsCount = closeDistanceAvatars.Count;
            for (var i = 0; i < closeDistanceAvatarsCount; i++)
            {
                IAvatarLODController currentAvatar = closeDistanceAvatars.Values[i];
                bool isLOD = i >= DataStore.i.avatarsLOD.maxNonLODAvatars.Get();
                if (isLOD)
                    currentAvatar.SetImpostorState();
                else
                {
                    if (closeDistanceAvatars.Keys[i] < SIMPLE_AVATAR_DISTANCE)
                        currentAvatar.SetAvatarState();
                    else
                        currentAvatar.SetSimpleAvatar();
                }
            }
        }

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