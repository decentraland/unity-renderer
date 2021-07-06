using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AvatarsLODController : MonoBehaviour
    {
        public static AvatarsLODController i { get; private set; }

        private const float LODS_LOCAL_Y_POS = 1.8f;
        private const float LODS_VERTICAL_MOVEMENT = 0.1f;
        private const float LODS_VERTICAL_MOVEMENT_DELAY = 1f;

        private List<AvatarShape> avatarsList = new List<AvatarShape>();
        private float lastLODsVerticalMovementTime = -1;

        private void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }
            i = this;

            enabled = false;
            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            // We need this check because in the CI the KernelConfig tests may have this object destroyed somehow
                            if (i == null || this == null)
                                return;

                            enabled = config.features.enableAvatarLODs;
                        });

            DataStore.i.avatarsLOD.LODDistance.OnChange += LODDistanceOnChange;
            DataStore.i.avatarsLOD.maxNonLODAvatars.OnChange += MaxNonLODAvatarsOnChange;
        }

        private void Update()
        {
            UpdateAllLODs();

            int listCount = avatarsList.Count;

            bool applyVerticalMovement = Time.timeSinceLevelLoad - lastLODsVerticalMovementTime > LODS_VERTICAL_MOVEMENT_DELAY;
            GameObject lodGO;
            for (int i = 0; i < listCount; i++)
            {
                lodGO = avatarsList[i].avatarRenderer.lodRenderer.gameObject;
                if (!lodGO.activeSelf)
                    continue;

                if (applyVerticalMovement)
                    lodGO.transform.localPosition = new Vector3(lodGO.transform.localPosition.x, (lodGO.transform.localPosition.y > LODS_LOCAL_Y_POS ? LODS_LOCAL_Y_POS : LODS_LOCAL_Y_POS + LODS_VERTICAL_MOVEMENT), lodGO.transform.localPosition.z);

                Vector3 previousForward = lodGO.transform.forward;
                Vector3 lookAtDir = (lodGO.transform.position - CommonScriptableObjects.cameraPosition).normalized;

                lookAtDir.y = previousForward.y;

                lodGO.transform.forward = lookAtDir;
            }

            if (applyVerticalMovement)
                lastLODsVerticalMovementTime = Time.timeSinceLevelLoad;
        }

        private void OnDestroy()
        {
            DataStore.i.avatarsLOD.LODDistance.OnChange -= LODDistanceOnChange;
            DataStore.i.avatarsLOD.maxNonLODAvatars.OnChange -= MaxNonLODAvatarsOnChange;
        }

        private void UpdateAllLODs()
        {
            if (!enabled)
                return;

            SortedList<float, AvatarShape> closeDistanceAvatars = new SortedList<float, AvatarShape>();
            foreach (AvatarShape avatar in avatarsList)
            {
                float distanceToPlayer = Vector3.Distance(CommonScriptableObjects.playerUnityPosition.Get(), avatar.transform.position);
                bool isInLODDistance = distanceToPlayer >= DataStore.i.avatarsLOD.LODDistance.Get();

                if (isInLODDistance)
                    ToggleLOD(avatar.avatarRenderer, true);
                else
                    closeDistanceAvatars.Add(distanceToPlayer, avatar);
            }

            int closeDistanceAvatarsCount = closeDistanceAvatars.Count;
            AvatarShape currentAvatar;
            for (var i = 0; i < closeDistanceAvatarsCount; i++)
            {
                currentAvatar = closeDistanceAvatars.Values[i];
                bool isLOD = i >= DataStore.i.avatarsLOD.maxNonLODAvatars.Get();

                ToggleLOD(currentAvatar.avatarRenderer, isLOD);
            }
        }

        public void RegisterAvatar(AvatarShape newAvatar)
        {
            if (!enabled || avatarsList.Contains(newAvatar))
                return;

            avatarsList.Add(newAvatar);
        }

        public void RemoveAvatar(AvatarShape targetAvatar)
        {
            if (!enabled || !avatarsList.Contains(targetAvatar))
                return;

            int listCount = avatarsList.Count;
            for (int i = 0; i < listCount; i++)
            {
                if (avatarsList[i] == targetAvatar)
                {
                    avatarsList.RemoveAt(i);

                    return;
                }
            }
        }

        private void ToggleLOD(AvatarRenderer avatarRenderer, bool enabled)
        {
            avatarRenderer.lodRenderer.gameObject.SetActive(enabled);
            avatarRenderer.SetVisibility(!enabled); // TODO: Resolve coping with AvatarModifierArea regarding this toggling (issue #718)
        }

        private void LODDistanceOnChange(float current, float previous) { UpdateAllLODs(); }

        private void MaxNonLODAvatarsOnChange(int current, int previous) { UpdateAllLODs(); }
    }
}