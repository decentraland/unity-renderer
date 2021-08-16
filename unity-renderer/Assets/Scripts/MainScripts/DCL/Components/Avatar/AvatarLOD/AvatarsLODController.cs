using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        internal const bool SORT_AVATARS = false;
        internal const float SIMPLE_AVATAR_DISTANCE = 10f;
        private const int DEFAULT_MAX_AVATAR = 70;
        private const int DEFAULT_MAX_IMPOSTORS = 200;

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

        internal void UpdateAllLODs(int maxAvatars = DEFAULT_MAX_AVATAR, int maxImpostors = DEFAULT_MAX_IMPOSTORS)
        {
            if (SORT_AVATARS)
                UpdateAllLodsSorted(maxAvatars, maxImpostors);
            else
                UpdateAllLodsNotSorted(maxAvatars, maxImpostors);

        }

        void UpdateAllLodsNotSorted(int maxAvatars = DEFAULT_MAX_AVATAR, int maxImpostors = DEFAULT_MAX_IMPOSTORS)
        {
            int avatarsCount = 0; //Full Avatar + Simple Avatar
            int impostorCount = 0; //Impostor

            Queue<IAvatarLODController> nearPlayersWithoutSpot = new Queue<IAvatarLODController>(); // Near players without an avatar spot
            Queue<IAvatarLODController> farawayPlayerPromoted = new Queue<IAvatarLODController>(); // List of faraway players promoted to avatars

            float lodDistance = DataStore.i.avatarsLOD.LODDistance.Get();
            foreach (IAvatarLODController lodController in lodControllers.Values)
            {
                float distance = DistanceToOwnPlayer(lodController.player);

                if (distance < 0) //Behind camera
                {
                    lodController.SetInvisible();
                    continue;
                }

                //Nearby player
                if (distance < lodDistance)
                {
                    if (avatarsCount < maxAvatars)
                    {
                        if (distance < SIMPLE_AVATAR_DISTANCE)
                            lodController.SetFullAvatar();
                        else
                            lodController.SetSimpleAvatar();
                        avatarsCount++;
                        continue;
                    }

                    // near avatar without spot will go invisible
                    nearPlayersWithoutSpot.Enqueue(lodController);
                    continue;
                }

                //We enqueue this faraway player as a candidate to be avatar instead of impostor
                if (avatarsCount < maxAvatars)
                {
                    farawayPlayerPromoted.Enqueue(lodController);
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
            EvaluatePromotedFarawayPlayers(nearPlayersWithoutSpot, farawayPlayerPromoted, maxImpostors - impostorCount);
        }

        internal void EvaluatePromotedFarawayPlayers(Queue<IAvatarLODController> nearPlayersWithoutSpot, Queue<IAvatarLODController> farAwayPlayersPromoted, int impostorSpotsLeft)
        {
            //Each promoted faraway player holds a spot for an avatar.
            //If we have near players without spots, we let them take the spot
            //Otherwise we give the spot of a Simple Avatar to the faraway player

            while (farAwayPlayersPromoted.Count > 0)
            {
                //We ran out of near players, we can claim the avatar spot for this faraway player
                if (nearPlayersWithoutSpot.Count <= 0)
                {
                    farAwayPlayersPromoted.Dequeue().SetSimpleAvatar();
                    continue;
                }

                //We claim back the avatar spot for the nearby player
                IAvatarLODController nearPlayer = nearPlayersWithoutSpot.Dequeue();
                if (DistanceToOwnPlayer(nearPlayer.player) < SIMPLE_AVATAR_DISTANCE)
                    nearPlayer.SetFullAvatar();
                else
                    nearPlayer.SetSimpleAvatar();

                if (impostorSpotsLeft > 0)
                {
                    farAwayPlayersPromoted.Dequeue().SetImpostor();
                    impostorSpotsLeft--;
                    continue;
                }
                farAwayPlayersPromoted.Dequeue().SetInvisible();
            }

            //We still have some near player to process but we have no more spots
            //we set them invisible
            while (nearPlayersWithoutSpot.Count > 0)
            {
                nearPlayersWithoutSpot.Dequeue().SetInvisible();
            }
        }

        void UpdateAllLodsSorted(int maxAvatars = DEFAULT_MAX_AVATAR, int maxImpostors = DEFAULT_MAX_IMPOSTORS)
        {
            int avatarsCount = 0; //Full Avatar
            int impostorCount = 0; //Impostor

            //Sort all the players by distance to maximize the amount of HQ avatars.
            IEnumerable<(IAvatarLODController lodController, float distance)> sortedPlayers = lodControllers.Select(x => (lodController: x.Value, distance: DistanceToOwnPlayer(x.Value.player))).OrderBy(x => x.distance).ToList();
            foreach ((IAvatarLODController lodController, float distance) in sortedPlayers)
            {
                if (distance < 0) //Behind camera
                {
                    lodController.SetInvisible();
                    continue;
                }

                if (avatarsCount < maxAvatars)
                {
                    if (distance < SIMPLE_AVATAR_DISTANCE)
                        lodController.SetFullAvatar();
                    else
                        lodController.SetSimpleAvatar();

                    avatarsCount++;
                    continue;
                }

                if (distance >= DataStore.i.avatarsLOD.LODDistance.Get() && (avatarsCount + impostorCount) < maxImpostors)
                {
                    lodController.SetImpostor();
                    impostorCount++;
                    continue;
                }

                lodController.SetInvisible();
            }
        }

        /// <summary>
        /// Returns -1 if player is not in front of camera or not found
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private float DistanceToOwnPlayer(Player player)
        {
            if (player == null || !IsInFrontOfCamera(player))
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