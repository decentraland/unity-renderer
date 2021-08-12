using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        private const float SIMPLE_AVATAR_DISTANCE = 10f;

        private readonly Dictionary<string, AvatarLODController> lodControllers = new Dictionary<string, AvatarLODController>();
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private Vector3 cameraForward;
        private Vector3 cameraPosition;
        private Vector3 mainPlayerPosition;
        private bool enabled;

        public AvatarsLODController()
        {
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
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
                        });
        }

        public void RegisterAvatar(string id, Player player)
        {
            if (!enabled || lodControllers.ContainsKey(id))
                return;

            lodControllers.Add(id, new AvatarLODController(player));
        }

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

            mainPlayerPosition = CommonScriptableObjects.playerUnityPosition.Get();
            cameraForward = CommonScriptableObjects.cameraForward.Get();
            cameraPosition = CommonScriptableObjects.cameraPosition.Get();

            UpdateAllLODs();
            UpdateLODsBillboard();
        }

        private void UpdateLODsBillboard()
        {
            foreach (var kvp in lodControllers)
            {
                otherPlayers.TryGetValue(kvp.Key, out Player player);

                if (!IsBeingRendered(player.worldPosition, cameraForward, cameraPosition))
                    continue;

                Vector3 previousForward = player.forwardDirection;
                Vector3 lookAtDir = (player.worldPosition - cameraPosition).normalized;

                lookAtDir.y = previousForward.y;
                player.renderer.SetImpostorForward(lookAtDir);
            }
        }

        private void UpdateAllLODs()
        {
            SortedList<float, AvatarLODController> renderedAvatars = new SortedList<float, AvatarLODController>();
            foreach (var avatarKVP in lodControllers)
            {
                var featureController = avatarKVP.Value;
                var position = otherPlayers[avatarKVP.Key].worldPosition;
                float distanceToPlayer = Vector3.Distance(mainPlayerPosition, position);

                if (IsBeingRendered(position, cameraForward, cameraPosition))
                {
                    while (renderedAvatars.ContainsKey(distanceToPlayer))
                    {
                        distanceToPlayer += 0.0001f;
                    }
                    renderedAvatars.Add(distanceToPlayer, featureController);
                }
                else
                {
                    featureController.SetImpostorState();
                }
            }

            int count = renderedAvatars.Count;
            int maxNonLODAvatars = DataStore.i.avatarsLOD.maxNonLODAvatars.Get();
            for (var i = 0; i < count; i++)
            {
                AvatarLODController currentAvatar = renderedAvatars.Values[i];
                bool isLOD = i >= maxNonLODAvatars;
                if (isLOD)
                {
                    currentAvatar.SetImpostorState();
                }
                else
                {
                    if (renderedAvatars.Keys[i] < SIMPLE_AVATAR_DISTANCE)
                        currentAvatar.SetAvatarState();
                    else
                        currentAvatar.SetSimpleAvatar();
                }
            }

            renderedAvatars.Clear();
        }

        private bool IsBeingRendered(Vector3 position, Vector3 cameraForward, Vector3 cameraPosition) { return Vector3.Dot(cameraForward, (position - cameraPosition).normalized) >= 0.25f; }

        public void Dispose()
        {
            foreach (AvatarLODController avatarFeaturesController in lodControllers.Values)
            {
                avatarFeaturesController.Dispose();
            }

            otherPlayers.OnAdded -= RegisterAvatar;
            otherPlayers.OnRemoved -= UnregisterAvatar;
        }
    }
}