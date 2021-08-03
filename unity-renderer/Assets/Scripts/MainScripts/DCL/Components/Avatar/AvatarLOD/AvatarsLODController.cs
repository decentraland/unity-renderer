using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : IAvatarsLODController
    {
        private const float SNAPSHOTS_FREQUENCY_IN_SECONDS = 2f;

        private List<AvatarLODController> avatarsList = new List<AvatarLODController>();

        public AvatarsLODController()
        {
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            DataStore.i.avatarsLOD.LODEnabled.Set(config.features.enableAvatarLODs);
                        });
        }

        public void Update()
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get())
                return;

            UpdateAllLODs();

            UpdateLODsBillboard();
        }

        public void RegisterAvatar(AvatarLODController newAvatar)
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get() || avatarsList.Contains(newAvatar) || newAvatar == null)
                return;

            avatarsList.Add(newAvatar);
        }

        public void RemoveAvatar(AvatarLODController targetAvatar)
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get() || !avatarsList.Contains(targetAvatar) || targetAvatar == null)
                return;

            int listCount = avatarsList.Count;
            for (int i = 0; i < listCount; i++)
            {
                if (avatarsList[i] == targetAvatar)
                {
                    avatarsList.RemoveAt(i);

                    targetAvatar.ToggleLOD(false);

                    return;
                }
            }
        }

        private void UpdateLODsBillboard()
        {
            int listCount = avatarsList.Count;
            GameObject lodGO;
            for (int i = 0; i < listCount; i++)
            {
                lodGO = avatarsList[i].GetImpostorMeshRenderer().gameObject;
                if (!lodGO.activeSelf)
                    continue;

                Vector3 previousForward = lodGO.transform.forward;
                Vector3 lookAtDir = (lodGO.transform.position - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;

                lodGO.transform.forward = lookAtDir;
            }
        }

        public void Dispose() { }

        private void UpdateAllLODs()
        {
            if (!DataStore.i.avatarsLOD.LODEnabled.Get())
                return;

            SortedList<float, AvatarLODController> closeDistanceAvatars = new SortedList<float, AvatarLODController>();
            foreach (AvatarLODController avatar in avatarsList)
            {
                float distanceToPlayer = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), avatar.GetTransform().position);
                float LODDistance = DataStore.i.avatarsLOD.LODDistance.Get();
                bool isInLODDistance = distanceToPlayer >= LODDistance;

                if (isInLODDistance)
                {
                    // If distance is farther than 2 LODDistance, we decrease its snapshots frequency
                    int distanceFrequencyMultiplier = distanceToPlayer < LODDistance * 2 ? 1 : 3;

                    ToggleAvatarLOD(avatar, true, SNAPSHOTS_FREQUENCY_IN_SECONDS * distanceFrequencyMultiplier);
                }
                else
                {
                    while (closeDistanceAvatars.ContainsKey(distanceToPlayer))
                    {
                        distanceToPlayer += 0.0001f;
                    }
                    closeDistanceAvatars.Add(distanceToPlayer, avatar);
                }
            }

            int closeDistanceAvatarsCount = closeDistanceAvatars.Count;
            AvatarLODController currentAvatar;
            for (var i = 0; i < closeDistanceAvatarsCount; i++)
            {
                currentAvatar = closeDistanceAvatars.Values[i];
                bool isLOD = i >= DataStore.i.avatarsLOD.maxNonLODAvatars.Get();

                ToggleAvatarLOD(currentAvatar, isLOD);
            }
        }

        private void ToggleAvatarLOD(AvatarLODController avatar, bool LODEnabled, float snapshotFrequency = SNAPSHOTS_FREQUENCY_IN_SECONDS)
        {
            bool shouldUpdateSnapshot = (Time.timeSinceLevelLoad - avatar.lastSnapshotsUpdateTime) >= SNAPSHOTS_FREQUENCY_IN_SECONDS;

            if (LODEnabled && shouldUpdateSnapshot)
                avatar.UpdateImpostorSnapshot();

            avatar.ToggleLOD(LODEnabled);

        }
    }
}